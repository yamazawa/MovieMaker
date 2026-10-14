using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MovieMaker.Converters;

/// <summary>
/// 全画面状態から、ウィンドウの最大化状態(WindowState)を求めるConverter
/// </summary>
public sealed class FullScreenWindowStateConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? WindowState.Maximized : WindowState.Normal;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
