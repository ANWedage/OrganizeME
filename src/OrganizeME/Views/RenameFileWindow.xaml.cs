using System.IO;
using System.Windows;
using System.Windows.Input;

namespace FolderFlow.Views;

public partial class RenameFileWindow : Window
{
    /// <summary>
    /// The file name (without path) the user chose, or null if they cancelled / kept original.
    /// The caller uses the original name when this is null.
    /// </summary>
    public string? NewFileName { get; private set; }

    private readonly string _originalFileName;

    public RenameFileWindow(string originalFileName)
    {
        InitializeComponent();
        _originalFileName = originalFileName;

        OriginalNameText.Text = $"Incoming file: {originalFileName}";

        // Pre-fill the text box with the name without extension so it's easy to edit.
        var nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
        FileNameBox.Text = nameWithoutExt;
        FileNameBox.SelectAll();
        FileNameBox.Focus();
    }

    private void Rename_Click(object sender, RoutedEventArgs e) => AcceptRename();

    private void KeepOriginal_Click(object sender, RoutedEventArgs e)
    {
        NewFileName = null;
        DialogResult = true;
    }

    private void FileNameBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) AcceptRename();
        else if (e.Key == Key.Escape) { NewFileName = null; DialogResult = true; }
    }

    private void AcceptRename()
    {
        var typed = FileNameBox.Text.Trim();
        if (string.IsNullOrEmpty(typed))
        {
            NewFileName = null;
            DialogResult = true;
            return;
        }

        // Re-attach original extension if the user didn't type one.
        var ext = Path.GetExtension(_originalFileName);
        NewFileName = typed.EndsWith(ext, StringComparison.OrdinalIgnoreCase)
            ? typed
            : typed + ext;

        DialogResult = true;
    }
}
