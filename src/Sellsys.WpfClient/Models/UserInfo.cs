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
