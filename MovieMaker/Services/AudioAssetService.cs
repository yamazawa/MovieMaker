using System.IO;

namespace MovieMaker.Services;

/// <summary>
/// 音声ファイル(BGM/SE)の元データを読み書きするサービス
///
/// %AppData%\MovieMaker(_Debug)\audio に選択時の拡張子のまま保存する。
/// </summary>
public class AudioAssetService
{
    private readonly string _assetsDirectory;

    public AudioAssetService()
    {
        _assetsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataPaths.FolderName, "audio");
        Directory.CreateDirectory(_assetsDirectory);
    }

    public string SaveAudio(string sourceFilePath)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(sourceFilePath)}";
        File.Copy(sourceFilePath, Path.Combine(_assetsDirectory, fileName));
        return fileName;
    }

    public Uri GetUri(string fileName) => new(Path.Combine(_assetsDirectory, fileName));
}
