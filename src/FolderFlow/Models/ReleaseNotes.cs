namespace FolderFlow.Models;

/// <summary>
/// Local release notes per version. Add a new entry here whenever you bump
/// the version in OrganizeME.csproj. These are used as a fallback when the
/// GitHub API body is empty (e.g. draft release or no internet).
/// </summary>
public static class ReleaseNotes
{
    private static readonly Dictionary<string, string> _notes = new()
    {
        
        ["1.0.5"] = """
            • Added a "View Release Notes" button in the settings window to view release notes for available updates.
            """,

        // ── Add new versions below ──────────────────────────────────────────
        // ["1.0.6"] = """
        //     • Description of new features or fixes
        //     """,
    };

    /// <summary>
    /// Returns the local release notes for the given version string (e.g. "1.0.2").
    /// Returns an empty string if no entry exists.
    /// </summary>
    public static string Get(string version) =>
        _notes.TryGetValue(version, out var notes) ? notes.Trim() : string.Empty;
}
