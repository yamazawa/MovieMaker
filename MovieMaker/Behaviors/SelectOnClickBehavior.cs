using System.Windows;
using System.Windows.Input;

namespace MovieMaker.Behaviors;

/// <summary>
/// 要素クリック時にCommandを実行する添付ビヘイビア(オブジェクトの選択に使用)
///
/// PreviewMouseLeftButtonDown(トンネリング)で処理するため、
/// DragMoveBehavior/ResizeHandleBehavior(バブリングのMouseLeftButtonDown)とは経路が別で干渉しない。
/// </summary>
public static class SelectOnClickBehavior
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(SelectOnClickBehavior),
            new PropertyMetadata(null, OnCommandChanged));

    public static void SetCommand(DependencyObject element, ICommand? value) => element.SetValue(CommandProperty, value);

    public static ICommand? GetCommand(DependencyObject element) => (ICommand?)element.GetValue(CommandProperty);

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        element.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;

        if (e.NewValue is not null)
            element.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var element = (FrameworkElement)sender;
        if (GetCommand(element) is { } command && command.CanExecute(element.DataContext))
            command.Execute(element.DataContext);
    }
}
