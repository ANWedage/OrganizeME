using System.Collections.Concurrent;
using System.IO;
using FolderFlow.Services.Interfaces;
using Serilog;

namespace FolderFlow.Services;

/// <summary>
/// Wraps .NET's FileSystemWatcher to detect new files in a folder.
/// Includes a small debounce/settle delay so we don't try to move a file
/// while the browser (or another app) is still writing to it.
/// </summary>
public class FileWatcherService : IFileWatcherService, IDisposable
{
    private readonly ILogger _logger;
    private FileSystemWatcher? _watcher;

    // Tracks files currently being "waited out" so we don't process the same
    // file twice if FileSystemWatcher fires multiple events for it.
    private readonly ConcurrentDictionary<string, byte> _pendingFiles = new();

    public event Func<string, Task>? FileDetected;

    public bool IsRunning => _watcher is { EnableRaisingEvents: true };

    public FileWatcherService(ILogger logger)
    {
        _logger = logger.ForContext<FileWatcherService>();
    }

    public void Start(string folderPath)
    {
        Stop();

        if (!Directory.Exists(folderPath))
        {
            _logger.Warning("Monitored folder does not exist: {Folder}", folderPath);
            Directory.CreateDirectory(folderPath);
        }

        _watcher = new FileSystemWatcher(folderPath)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            IncludeSubdirectories = false,
        };

        _watcher.Created += OnFileCreated;
        _watcher.Renamed += OnFileRenamed; // browsers often write ".tmp" then rename to final name
        _watcher.EnableRaisingEvents = true;

        _logger.Information("Started watching folder: {Folder}", folderPath);
    }

    public void Stop()
    {
        if (_watcher == null) return;

        _watcher.EnableRaisingEvents = false;
        _watcher.Created -= OnFileCreated;
        _watcher.Renamed -= OnFileRenamed;
        _watcher.Dispose();
        _watcher = null;

        _logger.Information("Stopped watching folder.");
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        _ = HandleCandidateFileAsync(e.FullPath);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        _ = HandleCandidateFileAsync(e.FullPath);
    }

    private async Task HandleCandidateFileAsync(string fullPath)
    {
        // Ignore directories — we only organize files.
        if (Directory.Exists(fullPath)) return;

        // Ignore browser partial-download artifacts.
        var ext = Path.GetExtension(fullPath).ToLowerInvariant();
        if (ext is ".tmp" or ".crdownload" or ".part" or ".download") return;

        if (!_pendingFiles.TryAdd(fullPath, 0)) return; // already being handled

        try
        {
            await WaitUntilFileIsReadyAsync(fullPath);

            if (!File.Exists(fullPath)) return; // disappeared (renamed/deleted) while waiting

            _logger.Information("New file detected: {File}", fullPath);

            if (FileDetected != null)
            {
                await FileDetected.Invoke(fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error handling detected file {File}", fullPath);
        }
        finally
        {
            _pendingFiles.TryRemove(fullPath, out _);
        }
    }

    /// <summary>
    /// Polls the file until its size stops changing and it can be opened
    /// exclusively, indicating the writer (e.g. browser) has finished.
    /// Times out after a reasonable window so we never hang forever.
    /// </summary>
    private static async Task WaitUntilFileIsReadyAsync(string path, int timeoutMs = 15000)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        long lastSize = -1;

        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            if (!File.Exists(path)) return;

            try
            {
                var size = new FileInfo(path).Length;
                using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
                stream.Close();

                if (size == lastSize && size > 0)
                {
                    return; // size stable across two checks and file is lockable -> ready
                }

                lastSize = size;
            }
            catch (IOException)
            {
                // Still locked by another process (still being written). Keep waiting.
            }

            await Task.Delay(500);
        }
    }

    public void Dispose() => Stop();
}
