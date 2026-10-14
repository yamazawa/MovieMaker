using System.Globalization;
using System.Windows.Data;
using MovieMaker.Resources;

namespace MovieMaker.Converters;

/// <summary>
/// 全画面状態から、全画面切替ボタンの表示文言を求めるConverter
/// </summary>
public sealed class FullScreenToggleLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Strings.Button_ExitFullScreen : Strings.Button_EnterFullScreen;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
