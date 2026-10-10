using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using MovieMaker.ViewModels;

namespace MovieMaker.Behaviors;

/// <summary>
/// タイムラインバーのドラッグ完了後に、開始時刻順の並び替えを反映させるヘルパー
///
/// タイムチャートはCollectionViewSourceで開始時刻の昇順に並べ替えているが、
/// ドラッグ中に毎回並び替わると操作しづらいためライブソートは行わず、
/// ドラッグ完了時にこのヘルパーで明示的にViewをRefreshする。
/// </summary>
internal static class TimelineSortHelper
{
    public static void RefreshSortOrder(DependencyObject element)
    {
        if (FindAncestorCanvas(element)?.DataContext is MainViewModel viewModel)
            CollectionViewSource.GetDefaultView(viewModel.Objects)?.Refresh();
    }

    private static Canvas? FindAncestorCanvas(DependencyObject element)
    {
        for (var current = VisualTreeHelper.GetParent(element); current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is Canvas canvas)
                return canvas;
        }

        return null;
    }
}
