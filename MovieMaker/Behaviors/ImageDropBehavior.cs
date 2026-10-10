using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MovieMaker.Behaviors;

/// <summary>
/// 要素へのファイルドラッグ&amp;ドロップで、画像ファイルのパスを渡してCommandを実行する添付ビヘイビア
///
/// ドロップされたファイルが複数ある場合は、画像として扱えるものそれぞれでCommandを実行する。
/// </summary>
public static class ImageDropBehavior
{
    private static readonly string[] ImageExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".gif"];

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(ImageDropBehavior),
            new PropertyMetadata(null, OnCommandChanged));

    public static void SetCommand(DependencyObject element, ICommand? value) => element.SetValue(CommandProperty, value);

    public static ICommand? GetCommand(DependencyObject element) => (ICommand?)element.GetValue(CommandProperty);

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        if (e.OldValue is not null)
        {
            element.PreviewDragOver -= OnPreviewDragOver;
            element.Drop -= OnDrop;
        }

        if (e.NewValue is not null)
        {
            element.AllowDrop = true;
            element.PreviewDragOver += OnPreviewDragOver;
            element.Drop += OnDrop;
        }
    }

    private static void OnPreviewDragOver(object sender, DragEventArgs e)
    {
        e.Effects = GetImageFilePaths(e.Data).Any() ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private static void OnDrop(object sender, DragEventArgs e)
    {
        var element = (FrameworkElement)sender;
        if (GetCommand(element) is not { } command)
            return;

        foreach (var path in GetImageFilePaths(e.Data))
        {
            if (command.CanExecute(path))
                command.Execute(path);
        }

        e.Handled = true;
    }

    private static IEnumerable<string> GetImageFilePaths(IDataObject data)
    {
        if (!data.GetDataPresent(DataFormats.FileDrop))
            return [];

        var paths = (string[])data.GetData(DataFormats.FileDrop);
        return paths.Where(path => ImageExtensions.Contains(Path.GetExtension(path).ToLowerInvariant()));
    }
}
