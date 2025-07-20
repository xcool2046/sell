using System;
using System.Globalization;
using System.Windows.Data;

namespace Sellsys.WpfClient.Converters
{
    /// <summary>
    /// 将字符串值转换为布尔值，用于RadioButton绑定
    /// </summary>
    public class StringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter != null)
            {
                return parameter.ToString();
            }

            return Binding.DoNothing;
        }
    }
}
