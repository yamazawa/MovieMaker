using System.Windows;
using MovieMaker.ViewModels;

namespace MovieMaker.Behaviors;

/// <summary>
/// 要素がフォーカスを得たタイミングでUndoスナップショットを記録する添付ビヘイビア
///
/// TextBox.Textの既定バインドはLostFocus確定のため、値が実際に変わるのは入力完了後になる。
/// そのため「変更後」ではなく「編集開始(フォーカス取得)」のタイミングで、変更前の状態を記録しておく。
/// </summary>
public static class UndoSnapshotOnFocusBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(UndoSnapshotOnFocusBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        if ((bool)e.NewValue)
            element.GotFocus += OnGotFocus;
        else
            element.GotFocus -= OnGotFocus;
    }

    private static void OnGotFocus(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is MainViewModel viewModel)
            viewModel.BeginUndoableChange();
    }
}
