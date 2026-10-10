using System.Windows;
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

        _viewModel = new MainViewModel();

        var mainWindow = new MainWindow { DataContext = _viewModel };
        MainWindow = mainWindow;
        mainWindow.Show();
    }
}

