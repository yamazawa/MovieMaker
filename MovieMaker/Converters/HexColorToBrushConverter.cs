using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MovieMaker.Converters;

/// <summary>
/// "#AARRGGBB"形式の文字列をSolidColorBrushに変換するConverter
/// </summary>
public sealed class HexColorToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string hex)
            return Brushes.Transparent;

        try
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }
        catch (FormatException)
        {
            return Brushes.Transparent;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
