using System.IO;

namespace FolderFlow.Models;

/// <summary>
/// Application settings, persisted to a JSON file in the user's AppData folder.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// The folder FolderFlow watches for new files. Defaults to the user's Downloads folder.
    /// </summary>
    public string MonitoredFolderPath { get; set; } = GetDefaultDownloadsPath();

    /// <summary>
    /// The root folder under which category subfolders (Documents, Pictures, etc.)
    /// are created. Defaults to the same folder being monitored, so e.g.
    /// Downloads\Documents, Downloads\Pictures are created alongside it.
    /// </summary>
    public string OrganizedRootPath { get; set; } = GetDefaultDownloadsPath();

    public bool StartWithWindows { get; set; } = false;

    public bool NotificationsEnabled { get; set; } = true;

    /// <summary>
    /// Master switch for AI-assisted categorization (Ollama). Off by default in V1
    /// since the AI integration is not implemented yet — this flag exists so the
    /// Settings UI and architecture are ready for Version 2.
    /// </summary>
    public bool AiEnabled { get; set; } = false;

    /// <summary>
    /// If false, FolderFlow will detect and log what it *would* do but won't
    /// actually move files. Useful for a first run / dry-run mode.
    /// </summary>
    public bool AutoMoveEnabled { get; set; } = true;

    public List<FolderRule> Rules { get; set; } = FolderRule_Defaults.GetDefaultRules();

    private static string GetDefaultDownloadsPath()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(userProfile, "Downloads");
    }
}

/// <summary>
/// Static provider of the default extension-to-category rules described in the
/// project spec. Kept separate from AppSettings so "Restore defaults" has a clean
/// source of truth to copy from.
/// </summary>
public static class FolderRule_Defaults
{
    public static List<FolderRule> GetDefaultRules() => new()
    {
        new FolderRule { Category = "Documents", Extensions = new() { "pdf", "doc", "docx", "txt", "xls", "xlsx", "ppt", "pptx" } },
        new FolderRule { Category = "Pictures", Extensions = new() { "jpg", "jpeg", "png", "gif", "webp" } },
        new FolderRule { Category = "Videos", Extensions = new() { "mp4", "mkv", "avi", "mov" } },
        new FolderRule { Category = "Music", Extensions = new() { "mp3", "wav", "flac" } },
        new FolderRule { Category = "Archives", Extensions = new() { "zip", "rar", "7z" } },
        new FolderRule { Category = "Applications", Extensions = new() { "exe", "msi" } },
        new FolderRule { Category = "Disk Images", Extensions = new() { "iso" } },
        new FolderRule { Category = "Data", Extensions = new() { "csv", "json", "xml" } },
        new FolderRule { Category = "Design", Extensions = new() { "psd", "fig", "ai" } },
    };
}
