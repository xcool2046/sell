using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sellsys.WpfClient.Converters
{
    /// <summary>
    /// 操作权限到可见性转换器
    /// 用于控制具体操作按钮的可见性
    /// </summary>
    public class OperationPermissionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string permissionString)
                return Visibility.Collapsed;

            // 解析权限字符串，格式：模块名:操作类型
            var parts = permissionString.Split(':');
            if (parts.Length != 2)
                return Visibility.Collapsed;

            string moduleName = parts[0];
            string operationName = parts[1];

            // 检查操作权限
            bool hasPermission = HasOperationPermission(moduleName, operationName);
            
            return hasPermission ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 检查操作权限
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <param name="operationName">操作名称</param>
        /// <returns>是否有权限</returns>
        private static bool HasOperationPermission(string moduleName, string operationName)
        {
            var currentUser = CurrentUser.User;
            if (currentUser == null)
                return false;

            // 首先检查模块权限
            if (!currentUser.HasPermission(moduleName))
                return false;

            // 管理员拥有所有操作权限
            if (currentUser.IsAdmin)
                return true;

            // 根据模块和操作类型进行具体权限判断
            return operationName.ToLower() switch
            {
                "view" => true, // 有模块权限就有查看权限
                "create" => HasCreatePermission(moduleName, currentUser),
                "edit" => HasEditPermission(moduleName, currentUser),
                "delete" => HasDeletePermission(moduleName, currentUser),
                _ => false
            };
        }

        /// <summary>
        /// 检查创建权限
        /// </summary>
        private static bool HasCreatePermission(string moduleName, UserInfo user)
        {
            // 根据具体业务规则判断
            return moduleName switch
            {
                "产品管理" => user.IsAdmin, // 只有管理员可以创建产品
                _ => true // 其他模块默认有创建权限
            };
        }

        /// <summary>
        /// 检查编辑权限
        /// </summary>
        private static bool HasEditPermission(string moduleName, UserInfo user)
        {
            // 根据具体业务规则判断
            return moduleName switch
            {
                "产品管理" => user.IsAdmin, // 只有管理员可以编辑产品
                _ => true // 其他模块默认有编辑权限
            };
        }

        /// <summary>
        /// 检查删除权限
        /// </summary>
        private static bool HasDeletePermission(string moduleName, UserInfo user)
        {
            // 根据具体业务规则判断
            return moduleName switch
            {
                "产品管理" => user.IsAdmin, // 只有管理员可以删除产品
                _ => true // 其他模块默认有删除权限
            };
        }
    }

    /// <summary>
    /// 操作权限到布尔值转换器
    /// 用于控制按钮的启用状态
    /// </summary>
    public class OperationPermissionToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string permissionString)
                return false;

            // 解析权限字符串，格式：模块名:操作类型
            var parts = permissionString.Split(':');
            if (parts.Length != 2)
                return false;

            string moduleName = parts[0];
            string operationName = parts[1];

            var currentUser = CurrentUser.User;
            if (currentUser == null)
                return false;

            // 首先检查模块权限
            if (!currentUser.HasPermission(moduleName))
                return false;

            // 管理员拥有所有操作权限
            if (currentUser.IsAdmin)
                return true;

            // 根据模块和操作类型进行具体权限判断
            return operationName.ToLower() switch
            {
                "view" => true,
                "create" => moduleName != "产品管理" || currentUser.IsAdmin,
                "edit" => moduleName != "产品管理" || currentUser.IsAdmin,
                "delete" => moduleName != "产品管理" || currentUser.IsAdmin,
                _ => false
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
