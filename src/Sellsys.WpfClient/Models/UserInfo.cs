using System.Collections.Generic;

namespace Sellsys.WpfClient.Models
{
    /// <summary>
    /// 当前登录用户信息
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// 用户ID（员工ID，管理员为0）
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 是否为管理员
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        public int? RoleId { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string? RoleName { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string? DepartmentName { get; set; }

        /// <summary>
        /// 分组ID
        /// </summary>
        public int? GroupId { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string? GroupName { get; set; }

        /// <summary>
        /// 可访问的模块列表
        /// </summary>
        public List<string> AccessibleModules { get; set; } = new List<string>();

        /// <summary>
        /// 检查是否有指定模块的访问权限
        /// </summary>
        /// <param name="module">模块名称</param>
        /// <returns>是否有权限</returns>
        public bool HasPermission(string module)
        {
            // 管理员拥有所有权限
            if (IsAdmin)
                return true;

            return AccessibleModules.Contains(module);
        }

        /// <summary>
        /// 获取角色级别
        /// </summary>
        /// <returns>角色级别</returns>
        public RoleLevel GetRoleLevel()
        {
            if (IsAdmin)
                return RoleLevel.Admin;

            if (string.IsNullOrEmpty(RoleName))
                return RoleLevel.Staff;

            // 根据角色名称判断级别
            if (RoleName.Contains("经理"))
                return RoleLevel.Manager;
            else if (RoleName.Contains("主管"))
                return RoleLevel.Supervisor;
            else
                return RoleLevel.Staff;
        }

        /// <summary>
        /// 检查是否可以查看指定分组的数据
        /// </summary>
        /// <param name="targetGroupId">目标分组ID</param>
        /// <returns>是否有权限</returns>
        public bool CanAccessGroupData(int? targetGroupId)
        {
            // 管理员可以访问所有分组数据
            if (IsAdmin)
                return true;

            // 如果目标分组为空，表示数据未分组，只有管理员可以访问
            if (!targetGroupId.HasValue)
                return IsAdmin;

            // 如果当前用户没有分组，只能访问自己的数据
            if (!GroupId.HasValue)
                return false;

            var roleLevel = GetRoleLevel();

            // 普通员工（Staff）不应该基于分组进行权限控制
            // 他们的权限应该在数据级别控制（即只能访问分配给自己的数据）
            // 这个方法主要用于主管和经理级别的分组权限判断
            if (roleLevel == RoleLevel.Staff)
                return false;

            // 主管和经理可以访问同分组的数据
            if (roleLevel == RoleLevel.Supervisor || roleLevel == RoleLevel.Manager)
                return GroupId == targetGroupId;

            return false;
        }

        /// <summary>
        /// 用户显示信息
        /// </summary>
        public string UserDisplayInfo
        {
            get
            {
                if (IsAdmin)
                    return $"{DisplayName} (管理员)";
                
                var info = DisplayName;
                if (!string.IsNullOrEmpty(RoleName))
                    info += $" - {RoleName}";
                if (!string.IsNullOrEmpty(DepartmentName))
                    info += $" ({DepartmentName})";
                
                return info;
            }
        }
    }
}
