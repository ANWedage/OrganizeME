using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FolderFlow.Helpers;

/// <summary>Converts a bool Success flag into a brush (green/red) for the history list.</summary>
public class SuccessToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var success = value is bool b && b;
        return success
            ? new SolidColorBrush(Color.FromRgb(0x2B, 0xAE, 0x66))
            : new SolidColorBrush(Color.FromRgb(0xE0, 0x47, 0x3E));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Converts a bool Success flag into a checkmark/cross symbol.</summary>
public class SuccessToSymbolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var success = value is bool b && b;
        return success ? "✓" : "✗";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Inverts a boolean — handy for IsEnabled bindings tied to the opposite of a flag.</summary>
public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => !(value is bool b && b);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => !(value is bool b && b);
}

/// <summary>Converts bool → Visibility (true = Visible, false = Collapsed).</summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v == Visibility.Visible;
}

/// <summary>Converts a string → Visibility (non-empty = Visible, empty/null = Collapsed).</summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => !string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Joins a List&lt;string&gt; of extensions into a readable ".pdf, .docx, .txt" string.</summary>
public class ExtensionsListConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<string> extensions)
        {
            return string.Join(", ", extensions.Select(e => "." + e));
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
