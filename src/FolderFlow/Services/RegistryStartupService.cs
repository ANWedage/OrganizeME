using FolderFlow.Services.Interfaces;
using Microsoft.Win32;
using Serilog;

namespace FolderFlow.Services;

/// <summary>
/// Registers/unregisters OrganizeME to launch automatically when Windows starts,
/// using the standard per-user Run registry key (no admin rights required).
/// </summary>
public class RegistryStartupService : IStartupService
{
    private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "OrganizeME";
    private readonly ILogger _logger;

    public RegistryStartupService(ILogger logger)
    {
        _logger = logger.ForContext<RegistryStartupService>();
    }

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        return key?.GetValue(AppName) != null;
    }

    public void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
                         ?? Registry.CurrentUser.CreateSubKey(RunKeyPath);

        if (enabled)
        {
            var exePath = Environment.ProcessPath ?? Process_GetFallbackPath();
            key.SetValue(AppName, $"\"{exePath}\" --minimized");
            _logger.Information("Enabled start-with-Windows.");
        }
        else
        {
            key.DeleteValue(AppName, throwOnMissingValue: false);
            _logger.Information("Disabled start-with-Windows.");
        }
    }

    private static string Process_GetFallbackPath() =>
        System.Reflection.Assembly.GetExecutingAssembly().Location;
}
