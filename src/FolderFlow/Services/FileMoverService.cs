using System.IO;
using FolderFlow.Services.Interfaces;
using Serilog;

namespace FolderFlow.Services;

public class FileMoverService : IFileMoverService
{
    private readonly ILogger _logger;

    public FileMoverService(ILogger logger)
    {
        _logger = logger.ForContext<FileMoverService>();
    }

    public async Task<string> MoveFileAsync(string sourceFilePath, string rootFolder, string category)
    {
        var destinationFolder = Path.Combine(rootFolder, category);

        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
            _logger.Information("Created folder: {Folder}", destinationFolder);
        }

        var fileName = Path.GetFileName(sourceFilePath);
        var destinationPath = Path.Combine(destinationFolder, fileName);

        destinationPath = GetCollisionSafePath(destinationPath);

        // File.Move can throw if the file is briefly locked; retry a couple of times.
        const int maxAttempts = 3;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await Task.Run(() => File.Move(sourceFilePath, destinationPath));
                _logger.Information("Moved {Source} -> {Destination}", sourceFilePath, destinationPath);
                return destinationPath;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                await Task.Delay(500);
            }
        }

        // Final attempt without catching, so a real failure bubbles up to the caller.
        await Task.Run(() => File.Move(sourceFilePath, destinationPath));
        return destinationPath;
    }

    /// <summary>
    /// If a file already exists at the target path, appends " (1)", " (2)", etc.
    /// until a free name is found.
    /// </summary>
    private static string GetCollisionSafePath(string path)
    {
        if (!File.Exists(path)) return path;

        var folder = Path.GetDirectoryName(path)!;
        var nameWithoutExt = Path.GetFileNameWithoutExtension(path);
        var ext = Path.GetExtension(path);

        var counter = 1;
        string candidate;
        do
        {
            candidate = Path.Combine(folder, $"{nameWithoutExt} ({counter}){ext}");
            counter++;
        } while (File.Exists(candidate));

        return candidate;
    }
}
