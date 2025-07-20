using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sellsys.WpfClient.Converters
{
    /// <summary>
    /// 将字符串值转换为Visibility，用于根据字符串值控制元素显示
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            // 如果值与参数相等，则显示，否则隐藏
            return value.ToString() == parameter.ToString() ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
