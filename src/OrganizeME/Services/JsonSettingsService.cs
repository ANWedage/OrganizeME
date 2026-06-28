using System.IO;
using System.Text.Json;
using FolderFlow.Models;
using FolderFlow.Services.Interfaces;
using Serilog;

namespace FolderFlow.Services;

public class JsonSettingsService : ISettingsService
{
    private readonly ILogger _logger;
    private readonly string _settingsPath;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public AppSettings Current { get; private set; } = new();

    public JsonSettingsService(ILogger logger)
    {
        _logger = logger.ForContext<JsonSettingsService>();

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, "OrganizeME");
        Directory.CreateDirectory(folder);
        _settingsPath = Path.Combine(folder, "settings.json");
    }

    public async Task LoadAsync()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = await File.ReadAllTextAsync(_settingsPath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                if (loaded != null)
                {
                    Current = loaded;
                    _logger.Information("Settings loaded from {Path}", _settingsPath);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load settings, using defaults.");
        }

        Current = new AppSettings();
        await SaveAsync(); // write defaults on first run
    }

    public async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(Current, JsonOptions);
            await File.WriteAllTextAsync(_settingsPath, json);
            _logger.Information("Settings saved.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save settings.");
        }
    }

    public void RestoreDefaultRules()
    {
        Current.Rules = FolderRule_Defaults.GetDefaultRules();
        _logger.Information("Folder rules restored to defaults.");
    }
}
