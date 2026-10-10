using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MovieMaker.Converters;

/// <summary>
/// 選択状態(bool)を、オブジェクト枠のBorderBrushに変換するConverter
/// </summary>
public sealed class SelectionBrushConverter : IValueConverter
{
    private static readonly Brush Selected = Brushes.Orange;
    private static readonly Brush Unselected = Brushes.LightGray;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Selected : Unselected;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
