using System.Windows;
using MovieMaker.Services;
using MovieMaker.ViewModels;

namespace MovieMaker;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private MainViewModel? _viewModel;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var clipboardImageService = new ClipboardImageService();
        _viewModel = new MainViewModel(clipboardImageService);

        var mainWindow = new MainWindow { DataContext = _viewModel };
        MainWindow = mainWindow;
        mainWindow.Show();
    }
}

