using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MovieMaker.Models;

namespace MovieMaker.Converters;

/// <summary>
/// SceneObjectTypeから、タイムラインバーの色を求めるConverter
///
/// 音声(BGM/SE)はキャンバス上の見た目を持たないため、タイムライン上で一目で区別できるよう色を変える。
/// </summary>
public sealed class SceneObjectTypeBrushConverter : IValueConverter
{
    private static readonly Brush VisualBrush = Brushes.SteelBlue;
    private static readonly Brush AudioBrush = Brushes.MediumPurple;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is SceneObjectType.Audio ? AudioBrush : VisualBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
