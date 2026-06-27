using FolderFlow.Models;

namespace FolderFlow.Services.Interfaces;

/// <summary>
/// Decides which category a file belongs to based on its extension and the
/// currently configured rules. This is the extension point Version 2's AI
/// categorizer will plug into later (see IAiCategorizer).
/// </summary>
public interface IFileCategorizer
{
    /// <summary>
    /// Returns the category name (e.g. "Documents") for the given file path,
    /// or null if no rule matches (the file should be left alone / put in "Other").
    /// </summary>
    string? Categorize(string filePath, IEnumerable<FolderRule> rules);
}
