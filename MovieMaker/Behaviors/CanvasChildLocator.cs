using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MovieMaker.Behaviors;

/// <summary>
/// 要素から、祖先のCanvasと、そのCanvasの直接の子(Canvas.Left/Topが効く要素)を探すヘルパー
///
/// オブジェクト一覧はItemsControl(ItemsPanel=Canvas)で表示するため、
/// ドラッグ・リサイズ対象のBorderはCanvasの直接の子ではなくContentPresenter経由になる。
/// Canvas.Left/Topは直接の子にしか効かないため、Visual木を辿ってContentPresenter側を特定する。
/// </summary>
internal static class CanvasChildLocator
{
    public static (FrameworkElement CanvasChild, Canvas Canvas)? Locate(DependencyObject element)
    {
        DependencyObject? walker = element;
        while (walker is not null)
        {
            var parent = VisualTreeHelper.GetParent(walker);
            if (parent is Canvas canvas && walker is FrameworkElement child)
                return (child, canvas);

            walker = parent;
        }

        return null;
    }
}
