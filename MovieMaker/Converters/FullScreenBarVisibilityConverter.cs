using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MovieMaker.Converters;

/// <summary>
/// 全画面状態から、上部の操作バーの表示/非表示を求めるConverter
///
/// 全画面中はアプリであることを感じさせないよう、ボタン類も含めて非表示にする。
/// </summary>
public sealed class FullScreenBarVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
