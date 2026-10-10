using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MovieMaker.Models;

namespace MovieMaker.Converters;

/// <summary>
/// SceneObjectTypeがConverterParameterで指定した種類名と一致する場合のみVisibleにするConverter
/// </summary>
public sealed class SceneObjectTypeVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SceneObjectType type || parameter is not string targetTypeName)
            return Visibility.Collapsed;

        return type.ToString() == targetTypeName ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
