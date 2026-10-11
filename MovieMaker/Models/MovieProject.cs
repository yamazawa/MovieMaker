namespace MovieMaker.Models;

/// <summary>
/// 動画プロジェクト全体の保存用データ
///
/// TimelinePixelsPerSecond/TimelineRowHeight/TimelinePanelHeight/CurrentTime/SelectedObjectId/
/// WindowWidth/WindowHeightは画面表示状態(次回起動時に復元する)。
/// Undo用のスナップショットとしてもこの型を使うが、Undoではこれらは書き戻さない(表示状態は編集操作の取り消し対象ではないため)。
/// </summary>
public sealed record MovieProject(
    double Duration,
    string BackgroundColorHex,
    double TimelinePixelsPerSecond,
    double TimelineRowHeight,
    double TimelinePanelHeight,
    double CurrentTime,
    Guid? SelectedObjectId,
    double WindowWidth,
    double WindowHeight,
    List<SceneObject> Objects);
