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

    // 新規オブジェクトの既定サイズ・配置。
    public const double DefaultObjectWidth = 300;
    public const double DefaultObjectHeight = 100;
    public const double DefaultFontSize = 48;
    public const double MinFontSize = 8;

    // 表示時間は0.1秒単位。開始と終了の間に最低限確保する間隔。
    public const double TimeStepSeconds = 0.1;

    // リサイズハンドルの表示サイズ(枠からのはみ出し量)。
    public const double ResizeHandleSize = 20;
    public const double ResizeHandleOverhang = 5;
    public static readonly GridLength ResizeHandleGridLength = new(ResizeHandleSize);

    public static readonly Thickness RootMargin = new(8);
    public static readonly Thickness SectionSpacing = new(4);
    public static readonly Thickness FieldSpacing = new(0, 0, 0, 4);
    public static readonly Thickness ElementSpacing = new(0, 0, 0, 12);
    public static readonly Thickness ButtonSpacing = new(0, 4, 8, 0);

    public static readonly FontFamily DefaultFontFamily = new("Meiryo");
}
