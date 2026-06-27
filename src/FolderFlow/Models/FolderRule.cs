namespace FolderFlow.Models;

/// <summary>
/// Represents one organizing rule: a category name (which becomes a folder)
/// mapped to a list of file extensions that belong in it.
/// </summary>
public class FolderRule
{
    /// <summary>
    /// The folder/category name, e.g. "Documents", "Pictures".
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Extensions without the dot, lowercase, e.g. "pdf", "docx".
    /// </summary>
    public List<string> Extensions { get; set; } = new();

    /// <summary>
    /// Whether this rule was added/edited by the user (true) or is a default (false).
    /// Used so "Restore defaults" only removes user-added rules.
    /// </summary>
    public bool IsCustom { get; set; }
}
