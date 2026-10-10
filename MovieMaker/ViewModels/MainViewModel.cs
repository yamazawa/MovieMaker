using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MovieMaker.Models;
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
    private void SelectObject(SceneObjectViewModel sceneObject)
    {
        foreach (var obj in Objects)
            obj.IsSelected = obj == sceneObject;

        SelectedObject = sceneObject;
    }
}
