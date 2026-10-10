namespace MovieMaker.Models;

/// <summary>
/// 画面モード
///
/// Createは配置・編集を行う作成モード、Playは作成済みの内容をループ再生する再生モード。
/// </summary>
public enum AppMode
{
    Create,
    Play,
}
