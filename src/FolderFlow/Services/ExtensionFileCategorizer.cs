using System.IO;
using FolderFlow.Models;
using FolderFlow.Services.Interfaces;

namespace FolderFlow.Services;

/// <summary>
/// Default V1 categorizer: matches the file's extension against the configured
/// FolderRule list. Version 2 can introduce an IAiCategorizer that wraps this
/// one and falls back to it when AI is disabled or uncertain.
/// </summary>
public class ExtensionFileCategorizer : IFileCategorizer
{
    public string? Categorize(string filePath, IEnumerable<FolderRule> rules)
    {
        var ext = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();

        if (string.IsNullOrEmpty(ext)) return null;

        foreach (var rule in rules)
        {
            if (rule.Extensions.Any(e => string.Equals(e, ext, StringComparison.OrdinalIgnoreCase)))
            {
                return rule.Category;
            }
        }

        return null; // No matching rule -> caller decides fallback (e.g. "Other")
    }
}
