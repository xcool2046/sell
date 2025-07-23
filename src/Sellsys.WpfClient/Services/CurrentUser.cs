using Sellsys.WpfClient.Models;
using System;

namespace Sellsys.WpfClient.Services
{
    /// <summary>
    /// 当前用户会话管理
    /// </summary>
    public static class CurrentUser
    {
        private static UserInfo? _currentUser;

        /// <summary>
        /// 当前登录的用户信息
        /// </summary>
        public static UserInfo? User
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                UserChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// 是否已登录
        /// </summary>
        public static bool IsLoggedIn => _currentUser != null;

        /// <summary>
        /// 是否为管理员
        /// </summary>
        public static bool IsAdmin => _currentUser?.IsAdmin ?? false;

        /// <summary>
        /// 用户信息变更事件
        /// </summary>
        public static event Action<UserInfo?>? UserChanged;

        /// <summary>
        /// 设置当前用户
        /// </summary>
        /// <param name="userInfo">用户信息</param>
        public static void SetUser(UserInfo userInfo)
        {
            User = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
        }

        /// <summary>
        /// 清除当前用户（登出）
        /// </summary>
        public static void ClearUser()
        {
            User = null;
        }

        /// <summary>
        /// 检查是否有指定模块的访问权限
        /// </summary>
        /// <param name="module">模块名称</param>
        /// <returns>是否有权限</returns>
        public static bool HasPermission(string module)
        {
            return _currentUser?.HasPermission(module) ?? false;
        }

        /// <summary>
        /// 获取用户显示信息
        /// </summary>
        /// <returns>用户显示信息</returns>
        public static string GetUserDisplayInfo()
        {
            return _currentUser?.UserDisplayInfo ?? "未登录";
        }
    }
}
