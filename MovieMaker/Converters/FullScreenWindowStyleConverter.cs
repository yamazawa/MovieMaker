using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MovieMaker.Converters;

/// <summary>
/// 全画面状態から、ウィンドウ枠の有無(WindowStyle)を求めるConverter
/// </summary>
public sealed class FullScreenWindowStyleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? WindowStyle.None : WindowStyle.SingleBorderWindow;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
