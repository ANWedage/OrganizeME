namespace FolderFlow.Services.Interfaces;

/// <summary>
/// Asks the user to rename a file before it is moved.
/// Returns the new file name (without path), or null if the user cancelled.
/// </summary>
public interface IRenamePromptService
{
    /// <param name="originalFileName">The file name as-is, e.g. "report.pdf".</param>
    /// <returns>New file name chosen by the user, or null to keep the original name.</returns>
    Task<string?> PromptRenameAsync(string originalFileName);
}
