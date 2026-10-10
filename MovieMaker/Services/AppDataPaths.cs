namespace MovieMaker.Services;

/// <summary>
/// アプリデータ(project.json・assets)の保存先フォルダ名
///
/// 本番(Release)とデバッグ実行(Debug)で保存先フォルダを分け、
/// 開発中の動作確認が本番データに影響しないようにする。
/// </summary>
public static class AppDataPaths
{
#if DEBUG
    public const string FolderName = "MovieMaker_Debug";
#else
    public const string FolderName = "MovieMaker";
#endif
}
