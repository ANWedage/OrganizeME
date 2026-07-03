using System.Windows;
using FolderFlow.Services.Interfaces;
using FolderFlow.Views;

namespace FolderFlow.Services;

/// <summary>
/// Shows <see cref="RenameFileWindow"/> on the UI thread and returns the user's choice.
/// </summary>
public class WpfRenamePromptService : IRenamePromptService
{
    public Task<string?> PromptRenameAsync(string originalFileName)
    {
        var tcs = new TaskCompletionSource<string?>();

        Application.Current.Dispatcher.Invoke(() =>
        {
            var owner = Application.Current.MainWindow;
            var window = new RenameFileWindow(originalFileName);
            if (owner?.IsLoaded == true)
                window.Owner = owner;

            window.Topmost = true;
            window.Activate();

            window.ShowDialog();
            tcs.SetResult(window.NewFileName);
        });

        return tcs.Task;
    }
}
