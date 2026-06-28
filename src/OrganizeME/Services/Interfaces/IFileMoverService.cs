namespace FolderFlow.Services.Interfaces;

/// <summary>
/// Safely moves a file into a destination folder, creating the folder if needed
/// and handling name collisions.
/// </summary>
public interface IFileMoverService
{
    /// <summary>
    /// Moves <paramref name="sourceFilePath"/> into a subfolder named
    /// <paramref name="category"/> under <paramref name="rootFolder"/>.
    /// Creates the destination folder if it doesn't exist.
    /// If a file with the same name already exists at the destination,
    /// a numeric suffix is appended, e.g. "report (1).pdf".
    /// </summary>
    /// <returns>The full path the file was moved to.</returns>
    Task<string> MoveFileAsync(string sourceFilePath, string rootFolder, string category);
}
