namespace FolderFlow.Services.Interfaces;

/// <summary>
/// Adds or removes FolderFlow from Windows startup (via the current-user Run registry key).
/// </summary>
public interface IStartupService
{
    bool IsEnabled();

    void SetEnabled(bool enabled);
}
