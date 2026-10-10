using System.Windows;
using System.Windows.Threading;
using MovieMaker.Services;
using MovieMaker.ViewModels;

namespace MovieMaker;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static readonly TimeSpan AutoSaveInterval = TimeSpan.FromSeconds(1);

    private MainViewModel? _viewModel;
    private DispatcherTimer? _autoSaveTimer;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var clipboardImageService = new ClipboardImageService();
        var imageAssetService = new ImageAssetService();
        var movieProjectService = new MovieProjectService();
        _viewModel = new MainViewModel(clipboardImageService, imageAssetService, movieProjectService);

        _autoSaveTimer = new DispatcherTimer { Interval = AutoSaveInterval };
        _autoSaveTimer.Tick += (_, _) => _viewModel.SaveIfDirty();
        _autoSaveTimer.Start();

        var mainWindow = new MainWindow { DataContext = _viewModel };
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _viewModel?.SaveIfDirty();
        base.OnExit(e);
    }
}
