using System.Globalization;
using System.Windows.Data;
using MovieMaker.Styles;

namespace MovieMaker.Converters;

/// <summary>
/// 秒数をタイムライン上のピクセル位置/幅に変換するConverter
/// </summary>
public sealed class SecondsToPixelsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is double seconds ? seconds * LayoutConstants.TimelinePixelsPerSecond : 0.0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
