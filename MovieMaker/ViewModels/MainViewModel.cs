using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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

    public MainViewModel(ClipboardImageService clipboardImageService)
    {
        _clipboardImageService = clipboardImageService;
        Objects.CollectionChanged += OnObjectsChanged;
    }

    [ObservableProperty]
    private AppMode _mode = AppMode.Create;

    [ObservableProperty]
    private double _currentTime;

    [ObservableProperty]
    private double _duration = LayoutConstants.DefaultDurationSeconds;

    [ObservableProperty]
    private string _backgroundColorHex = LayoutConstants.DefaultBackgroundColorHex;

    [ObservableProperty]
    private SceneObjectViewModel? _selectedObject;

    public ObservableCollection<SceneObjectViewModel> Objects { get; } = [];

    // 現在時刻に表示範囲が重なるオブジェクトのみ。キャンバスの表示にバインドする。
    public ObservableCollection<SceneObjectViewModel> VisibleObjects { get; } = [];

    [RelayCommand]
    private void ToggleMode() => Mode = Mode == AppMode.Create ? AppMode.Play : AppMode.Create;

    [RelayCommand]
    private void AddText()
    {
        var sceneObject = new SceneObjectViewModel(
            SceneObjectType.Text,
            x: (LayoutConstants.CanvasWidth - LayoutConstants.DefaultObjectWidth) / 2,
            y: (LayoutConstants.CanvasHeight - LayoutConstants.DefaultObjectHeight) / 2,
            width: LayoutConstants.DefaultObjectWidth,
            height: LayoutConstants.DefaultObjectHeight,
            startTime: 0,
            endTime: Duration)
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
            startTime: 0,
            endTime: Duration)
        {
            ImageSource = image,
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
    }

    private void OnSceneObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SceneObjectViewModel.StartTime) or nameof(SceneObjectViewModel.EndTime))
            RefreshVisibleObjects();
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
