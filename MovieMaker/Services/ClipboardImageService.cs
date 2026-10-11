using System.Windows;
using System.Windows.Media.Imaging;

namespace MovieMaker.Services;

/// <summary>
/// クリップボードの画像を取得するサービス
/// </summary>
public class ClipboardImageService
{
    public bool TryGetImage(out BitmapSource? image)
    {
        if (!Clipboard.ContainsImage())
        {
            image = null;
            return false;
        }

        image = Clipboard.GetImage();
        image?.Freeze();
        return image is not null;
    }
}
