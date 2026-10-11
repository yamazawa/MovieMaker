namespace MovieMaker.Models;

/// <summary>
/// 1オブジェクト(文字/画像)の保存用データ
///
/// 画像はAppData配下のassetsフォルダにファイルとして保存し、ここにはファイル名のみ持つ。
/// </summary>
public sealed record SceneObject(
    Guid Id,
    SceneObjectType Type,
    double X,
    double Y,
    double Width,
    double Height,
    double StartTime,
    double EndTime,
    string Text,
    double FontSize,
    string? ImageFileName);
