namespace FolderFlow.Services.Interfaces;

/// <summary>
/// Orchestrates the full pipeline for a single newly-detected file:
/// categorize -> move -> record history -> notify.
/// This is the "conductor" that IFileWatcherService's event calls into.
/// </summary>
public interface IFileOrganizerOrchestrator
{
    Task ProcessNewFileAsync(string filePath, bool suppressNotification = false);
    Task OrganizeExistingFilesAsync(string folderPath, IProgress<(int done, int total)>? progress = null, CancellationToken ct = default);
}
