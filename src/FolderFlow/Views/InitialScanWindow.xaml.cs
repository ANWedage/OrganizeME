using System.IO;
using System.Windows;
using FolderFlow.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FolderFlow.Views;

public partial class InitialScanWindow : Window
{
    private readonly string _folderPath;
    private CancellationTokenSource? _cts;

    public InitialScanWindow(string folderPath)
    {
        InitializeComponent();
        _folderPath = folderPath;
        FolderPathText.Text = folderPath;
    }

    private void OnSkipClicked(object sender, RoutedEventArgs e)
    {
        _cts?.Cancel();
        Close();
    }

    private async void OnOrganizeClicked(object sender, RoutedEventArgs e)
    {
        OrganizeButton.IsEnabled = false;
        SkipButton.IsEnabled = false;
        ProgressPanel.Visibility = Visibility.Visible;
        ProgressText.Text = "Starting…";

        _cts = new CancellationTokenSource();

        try
        {
            var orchestrator = App.Services.GetRequiredService<IFileOrganizerOrchestrator>();

            var progress = new Progress<(int done, int total)>(p =>
            {
                ProgressBar.Value = p.total > 0 ? (double)p.done / p.total * 100 : 0;
                ProgressText.Text = $"Organizing file {p.done} of {p.total}…";
            });

            await orchestrator.OrganizeExistingFilesAsync(_folderPath, progress, _cts.Token);

            ProgressText.Text = "Done! All files have been organized.";
            await Task.Delay(1200);
            Close();
        }
        catch (OperationCanceledException)
        {
            Close();
        }
        catch (Exception ex)
        {
            ProgressText.Text = $"Error: {ex.Message}";
            SkipButton.IsEnabled = true;
        }
    }
}
