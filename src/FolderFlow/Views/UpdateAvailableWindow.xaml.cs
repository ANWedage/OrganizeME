using System.Windows;
using FolderFlow.Models;
using FolderFlow.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FolderFlow.Views;

public partial class UpdateAvailableWindow : Window
{
    private readonly UpdateInfo _update;
    private bool _isDownloading;

    public UpdateAvailableWindow(UpdateInfo update)
    {
        InitializeComponent();

        _update = update;

        TitleText.Text = $"Version {update.Version.ToString(3)} is available!";
        SubtitleText.Text = $"You are running v{App.Services.GetRequiredService<IUpdateService>().CurrentVersion.ToString(3)}. A new version is ready to install.";

        ReleaseNotesText.Text = string.IsNullOrWhiteSpace(update.ReleaseNotes)
            ? ReleaseNotes.Get(update.Version.ToString(3))
            : update.ReleaseNotes;

        if (string.IsNullOrWhiteSpace(ReleaseNotesText.Text))
            ReleaseNotesText.Text = "No release notes provided.";
    }

    private async void OnDownloadClicked(object sender, RoutedEventArgs e)
    {
        if (_isDownloading) return;
        _isDownloading = true;

        DownloadButton.IsEnabled = false;
        LaterButton.IsEnabled = false;
        ProgressBar.Visibility = Visibility.Visible;
        ProgressBar.Value = 0;
        StatusText.Text = "Starting download…";

        try
        {
            var updateService = App.Services.GetRequiredService<IUpdateService>();
            var progress = new Progress<int>(p =>
            {
                ProgressBar.Value = p;
                StatusText.Text = $"Downloading… {p}%";
            });

            await updateService.DownloadAndInstallAsync(_update, progress);
            StatusText.Text = "Download complete — installer is launching.";
            await Task.Delay(1500);
            Close();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Download failed: {ex.Message}";
            DownloadButton.IsEnabled = true;
            LaterButton.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Collapsed;
            _isDownloading = false;
        }
    }

    private void OnLaterClicked(object sender, RoutedEventArgs e) => Close();
}
