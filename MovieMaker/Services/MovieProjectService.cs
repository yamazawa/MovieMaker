using System.IO;
using System.Text.Json;
using MovieMaker.Models;

namespace MovieMaker.Services;

/// <summary>
/// 動画プロジェクトを読み書きするサービス
///
/// %AppData%\MovieMaker(_Debug)\project.json に保存する。
/// </summary>
public class MovieProjectService
{
    private readonly string _projectPath;

    public MovieProjectService()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataPaths.FolderName);
        Directory.CreateDirectory(dir);
        _projectPath = Path.Combine(dir, "project.json");
    }

    public MovieProject? TryLoad()
    {
        if (!File.Exists(_projectPath))
            return null;

        try
        {
            return JsonSerializer.Deserialize<MovieProject>(File.ReadAllText(_projectPath));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public void Save(MovieProject project)
    {
        File.WriteAllText(_projectPath, JsonSerializer.Serialize(project));
    }
}
