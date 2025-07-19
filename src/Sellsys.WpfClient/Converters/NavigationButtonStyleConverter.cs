using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sellsys.WpfClient.Converters
{
    public class NavigationButtonStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentViewName = value as string;
            var buttonViewName = parameter as string;
            
            if (currentViewName == buttonViewName)
            {
                return Application.Current.FindResource("SelectedNavigationButtonStyle");
            }
            else
            {
                return Application.Current.FindResource("NavigationButtonStyle");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
