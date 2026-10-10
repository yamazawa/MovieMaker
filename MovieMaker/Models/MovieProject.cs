namespace MovieMaker.Models;

/// <summary>
/// 動画プロジェクト全体の保存用データ
///
/// TimelinePixelsPerSecond/TimelineRowHeightはタイムラインの拡縮状態(次回起動時に復元する表示設定)。
/// Undo用のスナップショットとしてもこの型を使うが、Undoではこの2つは書き戻さない(表示設定は編集操作の取り消し対象ではないため)。
/// </summary>
public sealed record MovieProject(
    double Duration,
    string BackgroundColorHex,
    double TimelinePixelsPerSecond,
    double TimelineRowHeight,
    List<SceneObject> Objects);
