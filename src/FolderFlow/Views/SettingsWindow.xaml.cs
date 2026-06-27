using System.Windows;
using FolderFlow.ViewModels;

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
}
