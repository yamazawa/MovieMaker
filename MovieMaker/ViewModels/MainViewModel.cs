using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MovieMaker.Models;
using MovieMaker.Styles;

namespace MovieMaker.ViewModels;

/// <summary>
/// メイン画面のViewModel
///
/// 作成モード・再生モードの切り替えと、両モードで共有するタイムライン状態(現在時刻・全体時間)を持つ。
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

    [RelayCommand]
    private void ToggleMode() => Mode = Mode == AppMode.Create ? AppMode.Play : AppMode.Create;
}
