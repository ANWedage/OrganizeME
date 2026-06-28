namespace FolderFlow.Services.Interfaces;

/// <summary>
/// Shows Windows toast notifications to the user.
/// </summary>
public interface INotificationService
{
    void ShowFileOrganized(string fileName, string category);

    void ShowError(string message);

    void ShowInfo(string title, string message);
}
