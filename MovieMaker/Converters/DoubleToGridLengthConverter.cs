using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MovieMaker.Converters;

/// <summary>
/// double(px)とGridLengthを相互変換するConverter
///
/// RowDefinition.HeightはGridLength型のため、ViewModel側のdoubleプロパティと
/// TwoWayバインドするために使う(GridSplitterのドラッグをViewModelへ反映するため)。
/// </summary>
public sealed class DoubleToGridLengthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        new GridLength(value is double d ? d : 0.0);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is GridLength length ? length.Value : 0.0;
}
