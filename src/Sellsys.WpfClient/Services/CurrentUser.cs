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

        /// <summary>
        /// 检查是否可以查看指定分组的数据
        /// </summary>
        /// <param name="targetGroupId">目标分组ID</param>
        /// <returns>是否有权限</returns>
        public static bool CanAccessGroupData(int? targetGroupId)
        {
            return _currentUser?.CanAccessGroupData(targetGroupId) ?? false;
        }

        /// <summary>
        /// 获取当前用户的角色级别
        /// </summary>
        /// <returns>角色级别</returns>
        public static Models.RoleLevel GetRoleLevel()
        {
            return _currentUser?.GetRoleLevel() ?? Models.RoleLevel.Staff;
        }

        /// <summary>
        /// 检查是否可以查看指定用户分配的数据
        /// </summary>
        /// <param name="assignedUserId">分配的用户ID</param>
        /// <param name="assignedUserGroupId">分配用户的分组ID</param>
        /// <returns>是否有权限</returns>
        public static bool CanAccessUserData(int? assignedUserId, int? assignedUserGroupId)
        {
            if (_currentUser == null)
                return false;

            // 管理员可以访问所有数据
            if (_currentUser.IsAdmin)
                return true;

            var roleLevel = _currentUser.GetRoleLevel();

            // 普通员工只能访问分配给自己的数据
            if (roleLevel == Models.RoleLevel.Staff)
                return assignedUserId == _currentUser.Id;

            // 主管和经理可以访问同分组的数据
            if (roleLevel == Models.RoleLevel.Supervisor || roleLevel == Models.RoleLevel.Manager)
                return _currentUser.CanAccessGroupData(assignedUserGroupId);

            return false;
        }

        /// <summary>
        /// 检查是否可以查看订单或售后数据（同组可见规则）
        /// </summary>
        /// <param name="assignedUserGroupId">分配用户的分组ID</param>
        /// <returns>是否有权限</returns>
        public static bool CanAccessOrderOrAfterSalesData(int? assignedUserGroupId)
        {
            if (_currentUser == null)
                return false;

            // 管理员可以访问所有数据
            if (_currentUser.IsAdmin)
                return true;

            // 订单和售后：同组可见（不区分角色级别）
            return _currentUser.CanAccessGroupData(assignedUserGroupId);
        }

        /// <summary>
        /// 检查是否有分配权限
        /// </summary>
        /// <returns>是否有分配权限</returns>
        public static bool CanAssignData()
        {
            if (_currentUser == null)
                return false;

            // 管理员拥有所有分配权限
            if (_currentUser.IsAdmin)
                return true;

            var roleLevel = _currentUser.GetRoleLevel();

            // 主管和经理级别可以进行分配操作
            return roleLevel == Models.RoleLevel.Supervisor || roleLevel == Models.RoleLevel.Manager;
        }

        /// <summary>
        /// 检查是否可以分配销售人员
        /// </summary>
        /// <returns>是否有权限</returns>
        public static bool CanAssignSales()
        {
            if (_currentUser == null)
                return false;

            // 管理员拥有所有分配权限
            if (_currentUser.IsAdmin)
                return true;

            // 销售经理和销售主管可以分配销售
            return _currentUser.RoleName?.Contains("销售") == true && CanAssignData();
        }

        /// <summary>
        /// 检查是否可以分配客服人员
        /// </summary>
        /// <returns>是否有权限</returns>
        public static bool CanAssignSupport()
        {
            if (_currentUser == null)
                return false;

            // 管理员拥有所有分配权限
            if (_currentUser.IsAdmin)
                return true;

            // 客服经理和客服主管可以分配客服
            return _currentUser.RoleName?.Contains("客服") == true && CanAssignData();
        }
    }
}
