using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeptunoApp.Converters;

/// <summary>
/// Muestra u oculta un elemento segun un booleano.
/// Con ConverterParameter="Invert" se invierte el resultado.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var valor = value is bool booleano && booleano;
        if (string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase))
        {
            valor = !valor;
        }
        return valor ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
