namespace FolderFlow.Services.Interfaces;

/// <summary>
/// Watches a folder for newly created files and raises an event for each one.
/// </summary>
public interface IFileWatcherService
{
    /// <summary>
    /// Raised whenever a new, fully-written file appears in the monitored folder.
    /// </summary>
    event Func<string, Task>? FileDetected;

    /// <summary>
    /// Starts watching the given folder. Safe to call again after a path change
    /// (it will stop the previous watcher first).
    /// </summary>
    void Start(string folderPath);

    void Stop();

    bool IsRunning { get; }
}
