using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using MovieMaker.Models;
using MovieMaker.Services;
using MovieMaker.Styles;

namespace MovieMaker.ViewModels;

/// <summary>
/// キャンバスに配置する1オブジェクト(文字/画像/音声)のViewModel
///
/// 位置・サイズ・表示時間はCanvas上のドラッグ・リサイズ・タイムラインから双方向に更新される。
/// 音声(BGM/SE)はキャンバス上の見た目を持たず、タイムライン上でのみ表示時間を編集する。
/// </summary>
public partial class SceneObjectViewModel : ObservableObject
{
    public Guid Id { get; }

    public SceneObjectType Type { get; }

    // 画像オブジェクトの保存ファイル名(assetsフォルダ内)。文字・音声オブジェクトではnull。
    public string? ImageFileName { get; set; }

    // 音声オブジェクトの保存ファイル名(audioフォルダ内)。文字・画像オブジェクトではnull。
    public string? AudioFileName { get; set; }

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

    // サイズを揃える機能で、揃えたいオブジェクトとして選ばれているか(パレットのチェックボックスで操作)。
    [ObservableProperty]
    private bool _isAlignTarget;

    // タイムライン上の表示区間バーの幅に使う(StartTime/EndTimeから算出)。
    public double DurationSeconds => EndTime - StartTime;

    // パレットの一覧表示用の短い表示名。
    public string DisplayLabel => Type switch
    {
        SceneObjectType.Text => string.IsNullOrWhiteSpace(Text) ? "(文字)" : Text,
        SceneObjectType.Audio => "(音声)",
        _ => "(画像)",
    };

    public SceneObjectViewModel(SceneObjectType type, double x, double y, double width, double height, double startTime, double endTime, Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        Type = type;
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _startTime = startTime;
        _endTime = endTime;
    }

    public static SceneObjectViewModel FromModel(SceneObject model, ImageAssetService imageAssetService)
    {
        var vm = new SceneObjectViewModel(model.Type, model.X, model.Y, model.Width, model.Height, model.StartTime, model.EndTime, model.Id)
        {
            Text = model.Text,
            FontSize = model.FontSize,
            ImageFileName = model.ImageFileName,
            AudioFileName = model.AudioFileName,
        };

        if (model.Type == SceneObjectType.Image && model.ImageFileName is not null)
            vm.ImageSource = imageAssetService.LoadImage(model.ImageFileName);

        return vm;
    }

    public SceneObject ToModel() => new(Id, Type, X, Y, Width, Height, StartTime, EndTime, Text, FontSize, ImageFileName, AudioFileName);

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

    partial void OnTextChanged(string value) => OnPropertyChanged(nameof(DisplayLabel));
}
