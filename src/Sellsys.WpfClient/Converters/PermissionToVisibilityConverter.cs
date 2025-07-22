using Sellsys.WpfClient.Services;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sellsys.WpfClient.Converters
{
    /// <summary>
    /// 权限到可见性转换器
    /// </summary>
    public class PermissionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string moduleName)
                return Visibility.Collapsed;

            // 检查当前用户是否有权限访问指定模块
            bool hasPermission = CurrentUser.HasPermission(moduleName);
            
            return hasPermission ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 权限到布尔值转换器
    /// </summary>
    public class PermissionToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string moduleName)
                return false;

            // 检查当前用户是否有权限访问指定模块
            return CurrentUser.HasPermission(moduleName);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 管理员权限到可见性转换器
    /// </summary>
    public class AdminPermissionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 只有管理员才能看到
            return CurrentUser.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
