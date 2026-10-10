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
    public const string DefaultBackgroundColorHex = "#FF000000";

    public const double PaletteWidth = 240;
    public const double TimelineHeight = 120;
    public const double DefaultDurationSeconds = 10.0;

    // 中央(パレット+キャンバス)とタイムラインの境界(GridSplitter)。
    public const double SplitterThickness = 6;
    public const double ContentMinHeight = 200;
    public const double TimelineMinHeight = 60;

    // 動画全体の長さ(秒)の編集可能範囲。最大10分。
    public const double MinDurationSeconds = 1.0;
    public const double MaxDurationSeconds = 600.0;

    // Grid.RowDefinition.HeightはGridLength型のため、double定数(TimelineHeight)とは別にGridLength版を用意する。
    public static readonly GridLength TimelineHeightGridLength = new(TimelineHeight);

    // 新規オブジェクトの既定サイズ・配置・表示時間(現在時刻から何秒間表示するか)。
    public const double DefaultObjectWidth = 300;
    public const double DefaultObjectHeight = 100;
    public const double DefaultObjectDurationSeconds = 4.0;
    public const double DefaultFontSize = 48;
    public const double MinFontSize = 8;

    // 表示時間は0.1秒単位。開始と終了の間に最低限確保する間隔。
    public const double TimeStepSeconds = 0.1;

    // タイムライン(スライドバー兼タイミングチャート)。横(秒→px)・縦(行の高さ)それぞれ拡縮可能。
    public const double TimelinePixelsPerSecond = 80;
    public const double TimelineZoomXMin = 10;
    public const double TimelineZoomXMax = 400;

    public const double TimelineRowHeight = 24;
    public const double TimelineZoomYMin = 12;
    public const double TimelineZoomYMax = 80;

    // タイムラインのクリック・ドラッグによる現在時刻の移動は1秒単位(0.1秒単位では細かすぎて
    // 特定の秒数ちょうどへ移動しづらいため)。
    public const double TimelineScrubStepSeconds = 1.0;

    // タイムラインバー端の開始/終了ドラッグハンドル幅(こちらは1秒単位で編集する)。
    public const double TimelineEdgeHandleWidth = 10;
    public static readonly GridLength TimelineEdgeHandleGridLength = new(TimelineEdgeHandleWidth);
    public const double TimelineEdgeStepSeconds = 1.0;

    // 再生モードのループ更新間隔(約30fps)。
    public const int PlaybackFrameIntervalMilliseconds = 33;

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
