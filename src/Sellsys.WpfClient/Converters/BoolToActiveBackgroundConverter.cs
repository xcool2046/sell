using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Sellsys.WpfClient.Converters
{
    public class BoolToActiveBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive && isActive)
            {
                // Active state - same as finance management order list color
                return new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
            }
            else
            {
                // Inactive state - same as finance management order list color
                return new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
