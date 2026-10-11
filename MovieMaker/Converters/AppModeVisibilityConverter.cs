using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MovieMaker.Models;

namespace MovieMaker.Converters;

/// <summary>
/// AppModeがConverterParameterで指定したモード名と一致する場合のみVisibleにするConverter
/// </summary>
public sealed class AppModeVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AppMode mode || parameter is not string targetModeName)
            return Visibility.Collapsed;

        return mode.ToString() == targetModeName ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
