namespace MovieMaker.Models;

/// <summary>
/// 1オブジェクト(文字/画像/音声)の保存用データ
///
/// 画像・音声はAppData配下のassets/audioフォルダにファイルとして保存し、ここにはファイル名のみ持つ。
/// 音声(BGM/SE)はキャンバス上の見た目を持たないため、X/Y/Width/Heightは使わない。
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
    string? ImageFileName,
    string? AudioFileName);
