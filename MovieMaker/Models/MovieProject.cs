namespace MovieMaker.Models;

/// <summary>
/// 動画プロジェクト全体の保存用データ
/// </summary>
public sealed record MovieProject(double Duration, string BackgroundColorHex, List<SceneObject> Objects);
