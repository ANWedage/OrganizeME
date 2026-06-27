using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FolderFlow.ViewModels;

/// <summary>
/// Base class for ViewModels. Provides the boilerplate for WPF data-binding
/// to know when a property's value has changed.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Sets the backing field and raises PropertyChanged only if the value actually changed.
    /// Usage: set { SetField(ref _myField, value); }
    /// </summary>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
