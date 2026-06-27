using FolderFlow.Models;

namespace FolderFlow.Services.Interfaces;

/// <summary>
/// Loads and saves AppSettings to/from a JSON file in the user's AppData folder.
/// </summary>
public interface ISettingsService
{
    AppSettings Current { get; }

    Task LoadAsync();

    Task SaveAsync();

    /// <summary>
    /// Replaces the current settings' rules with the built-in defaults
    /// (used by the "Restore defaults" button).
    /// </summary>
    void RestoreDefaultRules();
}
