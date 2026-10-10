using System.Windows;
using System.Windows.Input;
using MovieMaker.Styles;

namespace MovieMaker.Behaviors;

/// <summary>
/// タイムライン上のクリック・ドラッグ位置を現在時刻に変換する添付ビヘイビア(スライドバー本体)
///
/// クリックした瞬間から時刻を移動し、ドラッグ中も追従する。
/// SetCurrentValueで書き込むため、宣言済みの{Binding}を破壊せずに時刻を反映できる。
/// </summary>
public static class TimelineScrubBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(TimelineScrubBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.RegisterAttached("CurrentTime", typeof(double), typeof(TimelineScrubBehavior),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.RegisterAttached("Duration", typeof(double), typeof(TimelineScrubBehavior),
            new PropertyMetadata(1.0));

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    public static void SetCurrentTime(DependencyObject element, double value) => element.SetValue(CurrentTimeProperty, value);

    public static double GetCurrentTime(DependencyObject element) => (double)element.GetValue(CurrentTimeProperty);

    public static void SetDuration(DependencyObject element, double value) => element.SetValue(DurationProperty, value);

    public static double GetDuration(DependencyObject element) => (double)element.GetValue(DurationProperty);

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
        element.CaptureMouse();
        UpdateCurrentTime(element, e);
        e.Handled = true;
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        var element = (FrameworkElement)sender;
        if (element.IsMouseCaptured)
            UpdateCurrentTime(element, e);
    }

    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var element = (FrameworkElement)sender;
        element.ReleaseMouseCapture();
    }

    private static void UpdateCurrentTime(FrameworkElement element, MouseEventArgs e)
    {
        var duration = GetDuration(element);
        if (duration <= 0)
            return;

        var x = e.GetPosition(element).X;
        var time = Math.Clamp(x / LayoutConstants.TimelinePixelsPerSecond, 0, duration);
        element.SetCurrentValue(CurrentTimeProperty, Math.Round(time, 1));
    }
}
