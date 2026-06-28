using System.Collections.ObjectModel;
using FolderFlow.Helpers;
using FolderFlow.Models;
using FolderFlow.Services.Interfaces;

namespace FolderFlow.ViewModels;

/// <summary>
/// Backs the main window: shows whether monitoring is active, the monitored
/// folder, and a live feed of recently organized files.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IFileWatcherService _watcher;
    private readonly IFileOrganizerOrchestrator _orchestrator;
    private readonly ISettingsService _settings;
    private readonly IHistoryRepository _history;

    private string _statusText = "Stopped";
    private bool _isMonitoring;
    private string _monitoredFolder = string.Empty;

    public string StatusText
    {
        get => _statusText;
        set => SetField(ref _statusText, value);
    }

    public bool IsMonitoring
    {
        get => _isMonitoring;
        set => SetField(ref _isMonitoring, value);
    }

    public string MonitoredFolder
    {
        get => _monitoredFolder;
        set => SetField(ref _monitoredFolder, value);
    }

    public ObservableCollection<FileHistoryEntry> RecentActivity { get; } = new();

    public RelayCommand StartMonitoringCommand { get; }
    public RelayCommand StopMonitoringCommand { get; }
    public AsyncRelayCommand RefreshHistoryCommand { get; }

    public MainViewModel(
        IFileWatcherService watcher,
        IFileOrganizerOrchestrator orchestrator,
        ISettingsService settings,
        IHistoryRepository history)
    {
        _watcher = watcher;
        _orchestrator = orchestrator;
        _settings = settings;
        _history = history;

        StartMonitoringCommand = new RelayCommand(StartMonitoring, _ => !IsMonitoring);
        StopMonitoringCommand = new RelayCommand(StopMonitoring, _ => IsMonitoring);
        RefreshHistoryCommand = new AsyncRelayCommand(RefreshHistoryAsync);

        _watcher.FileDetected += OnFileDetectedAsync;
    }

    /// <summary>Call once after construction (e.g. from App startup) to load state.</summary>
    public async Task InitializeAsync()
    {
        MonitoredFolder = _settings.Current.MonitoredFolderPath;
        await RefreshHistoryAsync();
        StartMonitoring(null);
    }

    private void StartMonitoring(object? _)
    {
        MonitoredFolder = _settings.Current.MonitoredFolderPath;
        _watcher.Start(MonitoredFolder);
        IsMonitoring = true;
        StatusText = $"Watching: {MonitoredFolder}";
    }

    private void StopMonitoring(object? _)
    {
        _watcher.Stop();
        IsMonitoring = false;
        StatusText = "Stopped";
    }

    private async Task OnFileDetectedAsync(string filePath)
    {
        await _orchestrator.ProcessNewFileAsync(filePath);
        await RefreshHistoryAsync();
    }

    private async Task RefreshHistoryAsync()
    {
        var items = await _history.GetRecentAsync(50);

        // Must update the ObservableCollection on the UI thread.
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            RecentActivity.Clear();
            foreach (var item in items) RecentActivity.Add(item);
        });
    }
}
