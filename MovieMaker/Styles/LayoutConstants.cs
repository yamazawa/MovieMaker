using System.Windows;
using System.Windows.Media;

namespace MovieMaker.Styles;

/// <summary>
/// View全体で使うレイアウト関連の定数値
///
/// サイズ・余白を変更する際はここを直す。
/// </summary>
public static class LayoutConstants
{
    public const double WindowHeight = 800;
    public const double WindowWidth = 1280;
    public const double WindowMinHeight = 600;
    public const double WindowMinWidth = 960;

    // キャンバス(動画本体)の固定サイズ。
    public const double CanvasWidth = 1280;
    public const double CanvasHeight = 720;
    public const string DefaultBackgroundColorHex = "#FF202020";

    public const double PaletteWidth = 240;
    public const double TimelineHeight = 120;
    public const double DefaultDurationSeconds = 10.0;

    public static readonly Thickness RootMargin = new(8);
    public static readonly Thickness SectionSpacing = new(4);
    public static readonly Thickness FieldSpacing = new(0, 0, 0, 4);
    public static readonly Thickness ElementSpacing = new(0, 0, 0, 12);
    public static readonly Thickness ButtonSpacing = new(0, 4, 8, 0);

    public static readonly FontFamily DefaultFontFamily = new("Meiryo");
}
