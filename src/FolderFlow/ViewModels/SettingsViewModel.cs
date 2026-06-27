using System.Collections.ObjectModel;
using FolderFlow.Helpers;
using FolderFlow.Models;
using FolderFlow.Services.Interfaces;
using Microsoft.Win32;

namespace FolderFlow.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly IStartupService _startupService;
    private readonly IUpdateService _updateService;

    private string _monitoredFolderPath = string.Empty;
    private bool _startWithWindows;
    private bool _notificationsEnabled;
    private bool _aiEnabled;
    private bool _autoMoveEnabled;

    // ── Update-check state ───────────────────────────────────────────────────
    private string _updateStatus = string.Empty;
    private bool _isCheckingUpdate;
    private bool _isUpdateAvailable;
    private int _downloadProgress;
    private bool _isDownloading;
    private UpdateInfo? _pendingUpdate;
    public UpdateInfo? PendingUpdate => _pendingUpdate;

    public string UpdateStatus
    {
        get => _updateStatus;
        private set => SetField(ref _updateStatus, value);
    }

    public bool IsCheckingUpdate
    {
        get => _isCheckingUpdate;
        private set => SetField(ref _isCheckingUpdate, value);
    }

    public bool IsUpdateAvailable
    {
        get => _isUpdateAvailable;
        private set => SetField(ref _isUpdateAvailable, value);
    }

    public int DownloadProgress
    {
        get => _downloadProgress;
        private set => SetField(ref _downloadProgress, value);
    }

    public bool IsDownloading
    {
        get => _isDownloading;
        private set => SetField(ref _isDownloading, value);
    }

    public string CurrentVersionText => $"Current version: {_updateService.CurrentVersion.ToString(3)}";

    public string MonitoredFolderPath
    {
        get => _monitoredFolderPath;
        set => SetField(ref _monitoredFolderPath, value);
    }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set => SetField(ref _startWithWindows, value);
    }

    public bool NotificationsEnabled
    {
        get => _notificationsEnabled;
        set => SetField(ref _notificationsEnabled, value);
    }

    public bool AiEnabled
    {
        get => _aiEnabled;
        set => SetField(ref _aiEnabled, value);
    }

    public bool AutoMoveEnabled
    {
        get => _autoMoveEnabled;
        set => SetField(ref _autoMoveEnabled, value);
    }

    public ObservableCollection<FolderRule> Rules { get; } = new();

    public RelayCommand BrowseFolderCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand RestoreDefaultsCommand { get; }
    public AsyncRelayCommand CheckForUpdateCommand { get; }
    public AsyncRelayCommand DownloadUpdateCommand { get; }

    /// <summary>Raised after settings are saved, so MainViewModel can re-apply them (e.g. restart watcher).</summary>
    public event Action? SettingsSaved;

    public SettingsViewModel(ISettingsService settingsService, IStartupService startupService, IUpdateService updateService)
    {
        _settingsService = settingsService;
        _startupService = startupService;
        _updateService = updateService;

        BrowseFolderCommand = new RelayCommand(BrowseFolder);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        RestoreDefaultsCommand = new RelayCommand(RestoreDefaults);
        CheckForUpdateCommand = new AsyncRelayCommand(CheckForUpdateAsync);
        DownloadUpdateCommand = new AsyncRelayCommand(DownloadUpdateAsync);

        LoadFromSettings();
    }

    private void LoadFromSettings()
    {
        var s = _settingsService.Current;
        MonitoredFolderPath = s.MonitoredFolderPath;
        StartWithWindows = _startupService.IsEnabled();
        NotificationsEnabled = s.NotificationsEnabled;
        AiEnabled = s.AiEnabled;
        AutoMoveEnabled = s.AutoMoveEnabled;

        Rules.Clear();
        foreach (var rule in s.Rules) Rules.Add(rule);
    }

    private void BrowseFolder(object? _)
    {
        // OpenFolderDialog is available on .NET 8's Microsoft.Win32 namespace (Windows-only).
        var dialog = new OpenFolderDialog
        {
            Title = "Choose the folder OrganizeME should monitor",
            InitialDirectory = MonitoredFolderPath
        };

        if (dialog.ShowDialog() == true)
        {
            MonitoredFolderPath = dialog.FolderName;
        }
    }

    private void RestoreDefaults(object? _)
    {
        _settingsService.RestoreDefaultRules();
        Rules.Clear();
        foreach (var rule in _settingsService.Current.Rules) Rules.Add(rule);
    }

    private async Task CheckForUpdateAsync()
    {
        IsCheckingUpdate = true;
        IsUpdateAvailable = false;
        UpdateStatus = "Checking for updates…";
        _pendingUpdate = null;

        try
        {
            var update = await _updateService.CheckForUpdateAsync();
            if (update is null)
            {
                UpdateStatus = $"You're up to date! ({_updateService.CurrentVersion.ToString(3)})";
            }
            else
            {
                _pendingUpdate = update;
                IsUpdateAvailable = true;
                UpdateStatus = $"Version {update.Version.ToString(3)} is available!";
            }
        }
        catch
        {
            UpdateStatus = "Could not reach the update server. Check your connection.";
        }
        finally
        {
            IsCheckingUpdate = false;
        }
    }

    private async Task DownloadUpdateAsync()
    {
        if (_pendingUpdate is null) return;

        IsDownloading = true;
        DownloadProgress = 0;
        UpdateStatus = "Downloading update…";

        try
        {
            var progress = new Progress<int>(p =>
            {
                DownloadProgress = p;
                UpdateStatus = $"Downloading… {p}%";
            });

            await _updateService.DownloadAndInstallAsync(_pendingUpdate, progress);
            UpdateStatus = "Download complete — installer is launching.";
        }
        catch (Exception ex)
        {
            UpdateStatus = $"Download failed: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
        }
    }

    public async Task SaveAsync()
    {
        var s = _settingsService.Current;
        s.MonitoredFolderPath = MonitoredFolderPath;
        s.OrganizedRootPath = MonitoredFolderPath; // V1 keeps these the same
        s.NotificationsEnabled = NotificationsEnabled;
        s.AiEnabled = AiEnabled;
        s.AutoMoveEnabled = AutoMoveEnabled;
        s.Rules = Rules.ToList();

        _startupService.SetEnabled(StartWithWindows);

        await _settingsService.SaveAsync();

        SettingsSaved?.Invoke();
    }
}
