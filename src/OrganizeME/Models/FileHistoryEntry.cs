namespace FolderFlow.Models;

/// <summary>
/// Represents one row in the history database — a record of a single
/// file-organizing action FolderFlow performed (or attempted).
/// </summary>
public class FileHistoryEntry
{
    public int Id { get; set; }

    public string OriginalPath { get; set; } = string.Empty;

    public string NewPath { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string Extension { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.Now;

    public bool AiUsed { get; set; }

    public bool Success { get; set; }

    /// <summary>
    /// If Success is false, this holds the reason (e.g. "File in use", "Access denied").
    /// </summary>
    public string? ErrorMessage { get; set; }
}
