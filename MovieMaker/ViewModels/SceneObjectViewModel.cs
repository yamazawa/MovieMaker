using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using MovieMaker.Models;

namespace MovieMaker.ViewModels;

/// <summary>
/// キャンバスに配置する1オブジェクト(文字/画像)のViewModel
///
/// 位置・サイズ・表示時間はCanvas上のドラッグ・リサイズ・タイムラインから双方向に更新される。
/// </summary>
public partial class SceneObjectViewModel : ObservableObject
{
    public Guid Id { get; } = Guid.NewGuid();

    public SceneObjectType Type { get; }

    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    [ObservableProperty]
    private double _width;

    [ObservableProperty]
    private double _height;

    [ObservableProperty]
    private double _startTime;

    [ObservableProperty]
    private double _endTime;

    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private double _fontSize;

    [ObservableProperty]
    private BitmapSource? _imageSource;

    [ObservableProperty]
    private bool _isSelected;

    public SceneObjectViewModel(SceneObjectType type, double x, double y, double width, double height, double startTime, double endTime)
    {
        Type = type;
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _startTime = startTime;
        _endTime = endTime;
    }
}
