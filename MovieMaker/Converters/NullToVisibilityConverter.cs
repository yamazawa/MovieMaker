using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MovieMaker.Converters;

/// <summary>
/// 値がnullでない場合のみVisibleにするConverter(選択中オブジェクトがある場合だけ編集項目を表示する用途)
/// </summary>
public sealed class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is null ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
