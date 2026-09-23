using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeptunoApp.Converters;

/// <summary>Solo muestra el elemento cuando el texto enlazado tiene contenido.</summary>
public class StringNullOrEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
