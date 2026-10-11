using System.Globalization;
using System.Windows.Data;

namespace MovieMaker.Converters;

/// <summary>
/// 2つの数値(秒数×1秒あたりpx、行番号×行の高さ等)を掛け合わせるMultiValueConverter
///
/// タイムラインは横(秒→px)・縦(行の高さ)どちらも拡縮可能なため、
/// 固定倍率のConverterではなく2値を掛け合わせるMultiBinding用Converterにしてある。
/// </summary>
public sealed class MultiplyConverter : IMultiValueConverter
{
    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return 0.0;

        return ToDouble(values[0]) * ToDouble(values[1]);
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static double ToDouble(object? value) => value switch
    {
        double d => d,
        int i => i,
        _ => 0.0,
    };
}
