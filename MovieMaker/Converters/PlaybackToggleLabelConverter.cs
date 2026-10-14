using System.Globalization;
using System.Windows.Data;
using MovieMaker.Resources;

namespace MovieMaker.Converters;

/// <summary>
/// 再生中かどうかから、停止/再開ボタンの表示文言を求めるConverter
/// </summary>
public sealed class PlaybackToggleLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Strings.Button_PausePlayback : Strings.Button_ResumePlayback;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
