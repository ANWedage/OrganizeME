using FolderFlow.Services.Interfaces;
using Microsoft.Toolkit.Uwp.Notifications;
using Serilog;

namespace FolderFlow.Services;

public class ToastNotificationService : INotificationService
{
    private readonly ILogger _logger;
    private readonly ISettingsService _settingsService;

    public ToastNotificationService(ILogger logger, ISettingsService settingsService)
    {
        _logger = logger.ForContext<ToastNotificationService>();
        _settingsService = settingsService;
    }

    public void ShowFileOrganized(string fileName, string category)
    {
        if (!_settingsService.Current.NotificationsEnabled) return;

        try
        {
            new ToastContentBuilder()
                .AddText("OrganizeME organized a file")
                .AddText($"{fileName} → {category}")
                .Show();
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to show toast notification.");
        }
    }

    public void ShowError(string message)
    {
        if (!_settingsService.Current.NotificationsEnabled) return;

        try
        {
            new ToastContentBuilder()
                .AddText("OrganizeME ran into a problem")
                .AddText(message)
                .Show();
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to show error toast.");
        }
    }

    public void ShowInfo(string title, string message)
    {
        if (!_settingsService.Current.NotificationsEnabled) return;

        try
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to show info toast.");
        }
    }
}
