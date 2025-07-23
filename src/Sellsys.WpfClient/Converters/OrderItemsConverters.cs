using Sellsys.WpfClient.Models;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace Sellsys.WpfClient.Converters
{
    /// <summary>
    /// 将OrderItems集合转换为产品名称字符串
    /// </summary>
    public class OrderItemsToProductNamesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<OrderItem> orderItems && orderItems.Count > 0)
            {
                var names = orderItems.Select(item => item.ProductName).Where(name => !string.IsNullOrEmpty(name));
                return string.Join(", ", names);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 将OrderItems集合转换为规格字符串
    /// </summary>
    public class OrderItemsToSpecificationsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<OrderItem> orderItems && orderItems.Count > 0)
            {
                var specs = orderItems.Select(item => item.ProductSpecification).Where(spec => !string.IsNullOrEmpty(spec));
                return string.Join(", ", specs);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 将OrderItems集合转换为产品价格字符串
    /// </summary>
    public class OrderItemsToProductPricesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<OrderItem> orderItems && orderItems.Count > 0)
            {
                var prices = orderItems.Select(item => item.ProductPrice?.ToString("C") ?? "N/A");
                return string.Join(", ", prices);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 将OrderItems集合转换为实际价格字符串
    /// </summary>
    public class OrderItemsToActualPricesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<OrderItem> orderItems && orderItems.Count > 0)
            {
                var prices = orderItems.Select(item => item.ActualPrice.ToString("C"));
                return string.Join(", ", prices);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 将OrderItems集合转换为单位字符串
    /// </summary>
    public class OrderItemsToUnitsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<OrderItem> orderItems && orderItems.Count > 0)
            {
                var units = orderItems.Select(item => item.ProductUnit).Where(unit => !string.IsNullOrEmpty(unit));
                return string.Join(", ", units);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
