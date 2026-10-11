using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MovieMaker.Styles;
using MovieMaker.ViewModels;

namespace MovieMaker.Behaviors;

/// <summary>
/// タイムラインバー本体をドラッグする添付ビヘイビア
///
/// 横方向は表示時間の長さを保ったままStartTime/EndTimeを1秒単位で移動する。
/// 縦方向はマウスの現在位置(行)に応じて、MainViewModel.Objects内での並び順を入れ替える(表示行の入れ替え)。
/// 左右端のハンドル(TimelineEdgeDragBehavior)はe.Handled=trueで伝播を止めるため、
/// このビヘイビアはバー中央(ハンドル以外)のドラッグのみを対象とする。
/// </summary>
public static class TimelineBarMoveBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(TimelineBarMoveBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    private static readonly Dictionary<FrameworkElement, (Point Mouse, double StartTime, double EndTime)> DragOrigins = [];

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        if ((bool)e.NewValue)
        {
            element.MouseLeftButtonDown += OnMouseLeftButtonDown;
            element.MouseMove += OnMouseMove;
            element.MouseLeftButtonUp += OnMouseLeftButtonUp;
        }
        else
        {
            element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            element.MouseMove -= OnMouseMove;
            element.MouseLeftButtonUp -= OnMouseLeftButtonUp;
        }
    }

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var element = (FrameworkElement)sender;
        if (element.DataContext is not SceneObjectViewModel sceneObject || FindAncestor<Canvas>(element) is not { } canvas)
            return;

        if (canvas.DataContext is MainViewModel viewModel)
            viewModel.BeginUndoableChange();

        DragOrigins[element] = (e.GetPosition(canvas), sceneObject.StartTime, sceneObject.EndTime);
        element.CaptureMouse();
        e.Handled = true;
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        var element = (FrameworkElement)sender;
        if (!element.IsMouseCaptured || element.DataContext is not SceneObjectViewModel sceneObject ||
            FindAncestor<Canvas>(element) is not { } canvas || !DragOrigins.TryGetValue(element, out var origin))
            return;

        var pixelsPerSecond = TimelineScrubBehavior.GetPixelsPerSecond(canvas);
        var current = e.GetPosition(canvas);
        var deltaSeconds = Math.Round((current.X - origin.Mouse.X) / pixelsPerSecond / LayoutConstants.TimelineEdgeStepSeconds) * LayoutConstants.TimelineEdgeStepSeconds;

        var duration = origin.EndTime - origin.StartTime;
        var newStart = Math.Max(0, origin.StartTime + deltaSeconds);
        var newEnd = newStart + duration;

        // StartTime<EndTimeの不変条件を保つため、広がる側を先に反映する。
        if (newStart >= sceneObject.StartTime)
        {
            sceneObject.EndTime = newEnd;
            sceneObject.StartTime = newStart;
        }
        else
        {
            sceneObject.StartTime = newStart;
            sceneObject.EndTime = newEnd;
        }

        MoveToRowUnderMouse(canvas, sceneObject, current.Y);
    }

    // マウスのY座標が指す行へ、MainViewModel.Objects内でのオブジェクトの並び順を入れ替える。
    private static void MoveToRowUnderMouse(Canvas canvas, SceneObjectViewModel sceneObject, double mouseY)
    {
        if (canvas.DataContext is not MainViewModel viewModel || viewModel.TimelineRowHeight <= 0)
            return;

        var currentIndex = viewModel.Objects.IndexOf(sceneObject);
        if (currentIndex < 0)
            return;

        var targetIndex = Math.Clamp((int)(mouseY / viewModel.TimelineRowHeight), 0, viewModel.Objects.Count - 1);
        if (targetIndex != currentIndex)
            viewModel.Objects.Move(currentIndex, targetIndex);
    }

    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var element = (FrameworkElement)sender;
        element.ReleaseMouseCapture();
        DragOrigins.Remove(element);
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
