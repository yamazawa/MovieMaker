using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MovieMaker.Converters;

/// <summary>
/// 全画面状態から、ウィンドウのMaxWidth/MaxHeightを求めるConverter
///
/// WindowState=MaximizedはOSのタスクバーを避けた作業領域までしか広がらないため、
/// 全画面時はMaxWidth/MaxHeightをモニター全体のサイズにしてタスクバーも覆うようにする。
/// ConverterParameterに"Height"を指定した側は高さ、それ以外は幅を返す。
/// </summary>
public sealed class FullScreenMaxSizeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not true)
            return double.PositiveInfinity;

        return parameter as string == "Height" ? SystemParameters.PrimaryScreenHeight : SystemParameters.PrimaryScreenWidth;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
