using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using MovieMaker.Models;
using MovieMaker.Styles;

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

    // タイムライン上の表示区間バーの幅に使う(StartTime/EndTimeから算出)。
    public double DurationSeconds => EndTime - StartTime;

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

    // 表示時間は0.1秒単位に丸め、開始<終了・0秒以上を保つ。
    partial void OnStartTimeChanged(double value)
    {
        var clamped = Math.Round(Math.Max(0, Math.Min(value, EndTime - LayoutConstants.TimeStepSeconds)), 1);
        if (clamped != value)
            StartTime = clamped;

        OnPropertyChanged(nameof(DurationSeconds));
    }

    partial void OnEndTimeChanged(double value)
    {
        var clamped = Math.Round(Math.Max(value, StartTime + LayoutConstants.TimeStepSeconds), 1);
        if (clamped != value)
            EndTime = clamped;

        OnPropertyChanged(nameof(DurationSeconds));
    }

    partial void OnFontSizeChanged(double value)
    {
        var clamped = Math.Max(LayoutConstants.MinFontSize, value);
        if (clamped != value)
            FontSize = clamped;
    }
}
