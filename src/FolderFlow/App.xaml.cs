using System.IO;
using System.Windows;
using FolderFlow.Data;
using FolderFlow.Services;
using FolderFlow.Services.Interfaces;
using FolderFlow.ViewModels;
using FolderFlow.Views;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ILogger = Serilog.ILogger;

namespace FolderFlow;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    private TaskbarIcon? _trayIcon;
    private MainWindow? _mainWindow;
    private System.Threading.Mutex? _instanceMutex;

    protected override async void OnStartup(StartupEventArgs e)
    {
        _instanceMutex = new System.Threading.Mutex(true, "OrganizeME_SingleInstance", out bool isNewInstance);
        if (!isNewInstance)
        {
            MessageBox.Show("OrganizeME is already running.\nCheck the system tray icon.", "OrganizeME",
                MessageBoxButton.OK, MessageBoxImage.Information);
            _instanceMutex.Dispose();
            _instanceMutex = null;
            Environment.Exit(0);
            return;
        }

        base.OnStartup(e);

        ConfigureLogging();

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        var logger = Services.GetRequiredService<ILogger>();
        logger.Information("=== OrganizeME starting up ===");

        try
        {
            var settingsService = Services.GetRequiredService<ISettingsService>();
            await settingsService.LoadAsync();

            var historyRepo = Services.GetRequiredService<IHistoryRepository>();
            await historyRepo.InitializeAsync();

            SetupTrayIcon();

            _mainWindow = Services.GetRequiredService<MainWindow>();

            var startedMinimized = e.Args.Contains("--minimized");
            if (!startedMinimized)
            {
                _mainWindow.Show();
            }

            var mainViewModel = (MainViewModel)_mainWindow.DataContext;
            await mainViewModel.InitializeAsync();

            // Background update check — show a toast if a new version is available.
            _ = CheckForUpdateInBackgroundAsync();
        }
        catch (Exception ex)
        {
            logger.Fatal(ex, "Fatal error during startup.");
            MessageBox.Show($"OrganizeME failed to start:\n{ex.Message}", "OrganizeME",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    private void ConfigureLogging()
    {
        var logFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OrganizeME", "logs");
        Directory.CreateDirectory(logFolder);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .WriteTo.File(
                Path.Combine(logFolder, "organizeme-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14)
            .CreateLogger();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddSingleton(Log.Logger);

        // Core services — singletons because they hold state (watcher handle, settings, db path)
        // that should be shared across the whole app lifetime.
        services.AddSingleton<ISettingsService, JsonSettingsService>();
        services.AddSingleton<IFileWatcherService, FileWatcherService>();
        services.AddSingleton<IHistoryRepository, SqliteHistoryRepository>();
        services.AddSingleton<IStartupService, RegistryStartupService>();

        // Stateless-ish services — transient is fine, but singleton avoids re-allocating.
        services.AddSingleton<IFileCategorizer, ExtensionFileCategorizer>();
        services.AddSingleton<IFileMoverService, FileMoverService>();
        services.AddSingleton<INotificationService, ToastNotificationService>();
        services.AddSingleton<IFileOrganizerOrchestrator, FileOrganizerOrchestrator>();
        services.AddSingleton<IUpdateService, UpdateService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
        services.AddTransient<SettingsWindow>();
    }

    private void SetupTrayIcon()
    {
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "OrganizeME — organizing your downloads",
            Icon = new System.Drawing.Icon(
                Path.Combine(AppContext.BaseDirectory, "Resources", "Icons", "app.ico"))
        };

        var contextMenu = new System.Windows.Controls.ContextMenu();

        var openItem = new System.Windows.Controls.MenuItem { Header = "Open OrganizeME" };
        openItem.Click += (_, _) => ShowMainWindow();
        contextMenu.Items.Add(openItem);

        var exitItem = new System.Windows.Controls.MenuItem { Header = "Exit" };
        exitItem.Click += (_, _) => Shutdown();
        contextMenu.Items.Add(exitItem);

        _trayIcon.ContextMenu = contextMenu;
        _trayIcon.TrayMouseDoubleClick += (_, _) => ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        if (_mainWindow == null) return;
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private async Task CheckForUpdateInBackgroundAsync()
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5)); // let the app finish starting
            var updateService = Services.GetRequiredService<IUpdateService>();
            var update = await updateService.CheckForUpdateAsync();
            if (update is null) return;

            // Show the update dialog on the UI thread
            Current.Dispatcher.Invoke(() =>
            {
                var window = new FolderFlow.Views.UpdateAvailableWindow(update)
                {
                    Owner = _mainWindow
                };
                window.ShowDialog();
            });
        }
        catch
        {
            // Silently ignore — startup update check is best-effort.
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _instanceMutex?.ReleaseMutex();
        _instanceMutex?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
