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
                // Active state - darker blue
                return new SolidColorBrush(Color.FromRgb(0x1E, 0x4A, 0x7E));
            }
            else
            {
                // Inactive state - transparent
                return Brushes.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
