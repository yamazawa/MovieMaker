using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MovieMaker.Styles;

namespace MovieMaker.Converters;

/// <summary>
/// 全画面状態から、ルート要素のMarginを求めるConverter
///
/// 通常時はRootMarginを適用するが、全画面中は余白があると縁が黒塗りにならないため0にする。
/// </summary>
public sealed class FullScreenMarginConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? new Thickness(0) : LayoutConstants.RootMargin;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
