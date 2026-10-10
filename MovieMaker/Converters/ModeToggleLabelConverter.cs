using System.Globalization;
using System.Windows.Data;
using MovieMaker.Models;
using MovieMaker.Resources;

namespace MovieMaker.Converters;

/// <summary>
/// 現在のAppModeから、切替先モードへ遷移するボタンの表示文言を求めるConverter
/// </summary>
public sealed class ModeToggleLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is AppMode.Create ? Strings.Button_SwitchToPlayMode : Strings.Button_SwitchToCreateMode;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
