namespace Sellsys.Application.DTOs.Auth
{
    public class LoginResponseDto
    {
        /// <summary>
        /// 用户ID
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
        /// 可访问的模块列表（逗号分隔）
        /// </summary>
        public string AccessibleModules { get; set; } = string.Empty;
    }
}
