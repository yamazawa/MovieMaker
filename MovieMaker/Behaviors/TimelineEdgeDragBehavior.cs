using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MovieMaker.Styles;
using MovieMaker.ViewModels;

namespace MovieMaker.Behaviors;

/// <summary>
/// タイムラインバーの左右端をドラッグして、オブジェクトの表示開始/終了時刻を変更する添付ビヘイビア
///
/// StartTime/EndTimeはSceneObjectViewModelのプロパティ(DependencyPropertyではない)のため、
/// DragMoveBehavior等と異なりSetCurrentValueは使わず、DataContext経由で直接代入する。
/// 1秒単位に丸めて反映する(パレットでの手入力編集は0.1秒単位のまま変わらない)。
/// </summary>
public static class TimelineEdgeDragBehavior
{
    public static readonly DependencyProperty EdgeProperty =
        DependencyProperty.RegisterAttached("Edge", typeof(TimelineEdge?), typeof(TimelineEdgeDragBehavior),
            new PropertyMetadata(null, OnEdgeChanged));

    public static void SetEdge(DependencyObject element, TimelineEdge? value) => element.SetValue(EdgeProperty, value);

    public static TimelineEdge? GetEdge(DependencyObject element) => (TimelineEdge?)element.GetValue(EdgeProperty);

    private static void OnEdgeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        if (e.OldValue is not null)
        {
            element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            element.MouseMove -= OnMouseMove;
            element.MouseLeftButtonUp -= OnMouseLeftButtonUp;
        }

        if (e.NewValue is not null)
        {
            element.MouseLeftButtonDown += OnMouseLeftButtonDown;
            element.MouseMove += OnMouseMove;
            element.MouseLeftButtonUp += OnMouseLeftButtonUp;
        }
    }

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var handle = (FrameworkElement)sender;
        handle.CaptureMouse();

        // バー本体(SelectOnClickBehavior)・タイムライン本体(TimelineScrubBehavior)への伝播を止め、端のドラッグのみを行わせる。
        e.Handled = true;
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        var handle = (FrameworkElement)sender;
        if (!handle.IsMouseCaptured || GetEdge(handle) is not { } edge ||
            handle.DataContext is not SceneObjectViewModel sceneObject || FindAncestor<Canvas>(handle) is not { } canvas)
            return;

        var x = e.GetPosition(canvas).X;
        var pixelsPerSecond = TimelineScrubBehavior.GetPixelsPerSecond(canvas);
        var time = Math.Max(0, Math.Round(x / pixelsPerSecond / LayoutConstants.TimelineEdgeStepSeconds) * LayoutConstants.TimelineEdgeStepSeconds);

        if (edge == TimelineEdge.Start)
            sceneObject.StartTime = time;
        else
            sceneObject.EndTime = time;
    }

    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var handle = (FrameworkElement)sender;
        handle.ReleaseMouseCapture();
        TimelineSortHelper.RefreshSortOrder(handle);
    }

    private static T? FindAncestor<T>(DependencyObject element) where T : DependencyObject
    {
        for (var current = VisualTreeHelper.GetParent(element); current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is T match)
                return match;
        }

        return null;
    }
}
