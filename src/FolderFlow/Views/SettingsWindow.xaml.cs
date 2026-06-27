using System.Windows;
using FolderFlow.Services.Interfaces;
using FolderFlow.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FolderFlow.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;

    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private async void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveAsync();
        Close();
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnViewReleaseNotesClicked(object sender, RoutedEventArgs e)
    {
        var pendingUpdate = _viewModel.PendingUpdate;
        if (pendingUpdate is null) return;

        var window = new UpdateAvailableWindow(pendingUpdate) { Owner = this };
        window.ShowDialog();
    }
}
