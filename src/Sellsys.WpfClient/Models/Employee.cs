namespace Sellsys.WpfClient.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LoginUsername { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? BranchAccount { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? DepartmentName { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public DateTime CreatedAt { get; set; }

        // 显示用的格式化属性
        public string DisplayName => $"{Name} ({LoginUsername})";
        public string GroupInfo => !string.IsNullOrEmpty(GroupName) ? GroupName : "未分组";
        public string RoleInfo => !string.IsNullOrEmpty(RoleName) ? RoleName : "无角色";
        public string DepartmentInfo => !string.IsNullOrEmpty(DepartmentName) ? DepartmentName : "未分配部门";
        public string FormattedCreatedAt => CreatedAt.ToString("yyyy-MM-dd HH:mm");
    }
}
