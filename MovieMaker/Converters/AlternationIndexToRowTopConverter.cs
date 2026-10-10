using System.Globalization;
using System.Windows.Data;
using MovieMaker.Styles;

namespace MovieMaker.Converters;

/// <summary>
/// ItemsControl.AlternationIndexを、タイムライン上の行のY座標に変換するConverter
///
/// オブジェクトごとに1行を割り当て、重ならないよう縦に積む。
/// </summary>
public sealed class AlternationIndexToRowTopConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int index ? index * LayoutConstants.TimelineRowHeight : 0.0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
