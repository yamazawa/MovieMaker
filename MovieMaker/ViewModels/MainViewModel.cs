using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MovieMaker.Models;
using MovieMaker.Services;
using MovieMaker.Styles;

namespace MovieMaker.ViewModels;

/// <summary>
/// メイン画面のViewModel
///
/// 作成モード・再生モードの切り替え、配置済みオブジェクト一覧・選択状態、
/// 両モードで共有するタイムライン状態(現在時刻・全体時間)を持つ。
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ClipboardImageService _clipboardImageService;
    private readonly ImageAssetService _imageAssetService;
    private readonly MovieProjectService _movieProjectService;
    private readonly DispatcherTimer _playbackTimer;

    // 復元中はユーザー操作ではないため、MarkDirtyを抑制するためのガード。
    private bool _isRestoring;
    private bool _isDirty;
    private DateTime _lastPlaybackTick;

    public MainViewModel(ClipboardImageService clipboardImageService, ImageAssetService imageAssetService, MovieProjectService movieProjectService)
    {
        _clipboardImageService = clipboardImageService;
        _imageAssetService = imageAssetService;
        _movieProjectService = movieProjectService;
        Objects.CollectionChanged += OnObjectsChanged;

        _playbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(LayoutConstants.PlaybackFrameIntervalMilliseconds) };
        _playbackTimer.Tick += OnPlaybackTick;

        LoadProject();
    }

    /// <summary>
    /// 変更があれば動画プロジェクトを保存する(自動保存タイマーから呼ばれる)
    /// </summary>
    public void SaveIfDirty()
    {
        if (!_isDirty)
            return;

        _movieProjectService.Save(new MovieProject(Duration, BackgroundColorHex, Objects.Select(obj => obj.ToModel()).ToList()));
        _isDirty = false;
    }

    private void LoadProject()
    {
        var project = _movieProjectService.TryLoad();
        if (project is null)
            return;

        _isRestoring = true;
        Duration = project.Duration;
        BackgroundColorHex = project.BackgroundColorHex;
        foreach (var model in project.Objects)
            Objects.Add(SceneObjectViewModel.FromModel(model, _imageAssetService));
        _isRestoring = false;

        RefreshVisibleObjects();
    }

    private void MarkDirty()
    {
        if (!_isRestoring)
            _isDirty = true;
    }

    [ObservableProperty]
    private AppMode _mode = AppMode.Create;

    [ObservableProperty]
    private double _currentTime;

    [ObservableProperty]
    private double _duration = LayoutConstants.DefaultDurationSeconds;

    [ObservableProperty]
    private string _backgroundColorHex = LayoutConstants.DefaultBackgroundColorHex;

    // タイムラインの拡縮率(横:1秒あたりpx数、縦:1行の高さ)。
    [ObservableProperty]
    private double _timelinePixelsPerSecond = LayoutConstants.TimelinePixelsPerSecond;

    [ObservableProperty]
    private double _timelineRowHeight = LayoutConstants.TimelineRowHeight;

    [ObservableProperty]
    private SceneObjectViewModel? _selectedObject;

    public ObservableCollection<SceneObjectViewModel> Objects { get; } = [];

    // 現在時刻に表示範囲が重なるオブジェクトのみ。キャンバスの表示にバインドする。
    public ObservableCollection<SceneObjectViewModel> VisibleObjects { get; } = [];

    [RelayCommand]
    private void ToggleMode() => Mode = Mode == AppMode.Create ? AppMode.Play : AppMode.Create;

    // モード切替時、現在時刻は引き継ぐ(作成モードの再生位置から再生開始し、作成モードに戻った時もその時刻のまま)。
    partial void OnModeChanged(AppMode value)
    {
        if (value == AppMode.Play)
        {
            _lastPlaybackTick = DateTime.UtcNow;
            _playbackTimer.Start();
        }
        else
        {
            _playbackTimer.Stop();
        }
    }

    private void OnPlaybackTick(object? sender, EventArgs e)
    {
        if (Duration <= 0)
            return;

        var now = DateTime.UtcNow;
        var elapsed = (now - _lastPlaybackTick).TotalSeconds;
        _lastPlaybackTick = now;

        var next = CurrentTime + elapsed;
        CurrentTime = next >= Duration ? next % Duration : next;
    }

    [RelayCommand]
    private void AddText()
    {
        var sceneObject = new SceneObjectViewModel(
            SceneObjectType.Text,
            x: (LayoutConstants.CanvasWidth - LayoutConstants.DefaultObjectWidth) / 2,
            y: (LayoutConstants.CanvasHeight - LayoutConstants.DefaultObjectHeight) / 2,
            width: LayoutConstants.DefaultObjectWidth,
            height: LayoutConstants.DefaultObjectHeight,
            startTime: CurrentTime,
            endTime: CurrentTime + LayoutConstants.DefaultObjectDurationSeconds)
        {
            FontSize = LayoutConstants.DefaultFontSize,
        };

        Objects.Add(sceneObject);
        SelectObject(sceneObject);
    }

    [RelayCommand]
    private void AddImageFromClipboard()
    {
        if (!_clipboardImageService.TryGetImage(out var image) || image is null)
            return;

        var maxWidth = LayoutConstants.CanvasWidth * 0.6;
        var maxHeight = LayoutConstants.CanvasHeight * 0.6;
        var scale = Math.Min(1.0, Math.Min(maxWidth / image.PixelWidth, maxHeight / image.PixelHeight));
        var width = image.PixelWidth * scale;
        var height = image.PixelHeight * scale;

        var sceneObject = new SceneObjectViewModel(
            SceneObjectType.Image,
            x: (LayoutConstants.CanvasWidth - width) / 2,
            y: (LayoutConstants.CanvasHeight - height) / 2,
            width: width,
            height: height,
            startTime: CurrentTime,
            endTime: CurrentTime + LayoutConstants.DefaultObjectDurationSeconds)
        {
            ImageSource = image,
            ImageFileName = _imageAssetService.SaveImage(image),
        };

        Objects.Add(sceneObject);
        SelectObject(sceneObject);
    }

    [RelayCommand]
    private void SelectObject(SceneObjectViewModel sceneObject)
    {
        foreach (var obj in Objects)
            obj.IsSelected = obj == sceneObject;

        SelectedObject = sceneObject;
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        if (SelectedObject is null)
            return;

        Objects.Remove(SelectedObject);
        SelectedObject = null;
    }

    partial void OnCurrentTimeChanged(double value) => RefreshVisibleObjects();

    // 動画の長さは1秒～10分の範囲にクランプする。
    partial void OnDurationChanged(double value)
    {
        var clamped = Math.Clamp(Math.Round(value, 1), LayoutConstants.MinDurationSeconds, LayoutConstants.MaxDurationSeconds);
        if (clamped != value)
        {
            Duration = clamped;
            return;
        }

        MarkDirty();
    }

    partial void OnTimelinePixelsPerSecondChanged(double value)
    {
        var clamped = Math.Clamp(value, LayoutConstants.TimelineZoomXMin, LayoutConstants.TimelineZoomXMax);
        if (clamped != value)
            TimelinePixelsPerSecond = clamped;
    }

    partial void OnTimelineRowHeightChanged(double value)
    {
        var clamped = Math.Clamp(value, LayoutConstants.TimelineZoomYMin, LayoutConstants.TimelineZoomYMax);
        if (clamped != value)
            TimelineRowHeight = clamped;
    }

    private void OnObjectsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (SceneObjectViewModel item in e.OldItems)
                item.PropertyChanged -= OnSceneObjectPropertyChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (SceneObjectViewModel item in e.NewItems)
                item.PropertyChanged += OnSceneObjectPropertyChanged;
        }

        RefreshVisibleObjects();
        MarkDirty();
    }

    private void OnSceneObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SceneObjectViewModel.StartTime) or nameof(SceneObjectViewModel.EndTime))
            RefreshVisibleObjects();

        MarkDirty();
    }

    private void RefreshVisibleObjects()
    {
        var visible = Objects.Where(obj => obj.StartTime <= CurrentTime && CurrentTime < obj.EndTime).ToList();

        for (var i = VisibleObjects.Count - 1; i >= 0; i--)
        {
            if (!visible.Contains(VisibleObjects[i]))
                VisibleObjects.RemoveAt(i);
        }

        foreach (var obj in visible)
        {
            if (!VisibleObjects.Contains(obj))
                VisibleObjects.Add(obj);
        }
    }
}
