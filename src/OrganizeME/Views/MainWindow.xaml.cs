using System.Windows;
using FolderFlow.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FolderFlow.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnSettingsClicked(object sender, RoutedEventArgs e)
    {
        var settingsWindow = App.Services.GetRequiredService<SettingsWindow>();
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }

    /// <summary>
    /// Closing the window just hides it (FolderFlow keeps running in the tray).
    /// Use the tray icon's "Exit" option to actually quit.
    /// </summary>
    private void OnFeedbackClicked(object sender, RoutedEventArgs e)
    {
        var feedbackWindow = new FeedbackWindow();
        feedbackWindow.Owner = this;
        feedbackWindow.ShowDialog();
    }

    private void MainWindow_OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}
