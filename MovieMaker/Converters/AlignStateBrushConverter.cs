using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MovieMaker.Converters;

/// <summary>
/// 選択状態(IsSelected)・サイズ揃えの対象選択状態(IsAlignTarget)から、オブジェクト枠のBorderBrushを求めるMultiValueConverter
///
/// IsSelectedが優先(基準オブジェクトを示す)、次いでIsAlignTarget(揃える対象として選択中)、どちらでもなければ通常色。
/// </summary>
public sealed class AlignStateBrushConverter : IMultiValueConverter
{
    private static readonly Brush Selected = Brushes.Orange;
    private static readonly Brush AlignTarget = Brushes.LimeGreen;
    private static readonly Brush Normal = Brushes.LightGray;

    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length > 0 && values[0] is true)
            return Selected;

        if (values.Length > 1 && values[1] is true)
            return AlignTarget;

        return Normal;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
