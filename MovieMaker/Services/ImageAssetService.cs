using System.IO;
using System.Windows.Media.Imaging;

namespace MovieMaker.Services;

/// <summary>
/// 画像オブジェクトの元データを読み書きするサービス
///
/// %AppData%\MovieMaker(_Debug)\assets にPNGとして保存する。
/// </summary>
public class ImageAssetService
{
    private readonly string _assetsDirectory;

    public ImageAssetService()
    {
        _assetsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataPaths.FolderName, "assets");
        Directory.CreateDirectory(_assetsDirectory);
    }

    public string SaveImage(BitmapSource image)
    {
        var fileName = $"{Guid.NewGuid()}.png";

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));

        using var stream = File.Create(Path.Combine(_assetsDirectory, fileName));
        encoder.Save(stream);

        return fileName;
    }

    public BitmapImage LoadImage(string fileName)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(Path.Combine(_assetsDirectory, fileName));
        image.EndInit();
        image.Freeze();
        return image;
    }
}
