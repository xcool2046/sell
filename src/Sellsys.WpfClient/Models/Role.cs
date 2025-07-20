namespace Sellsys.WpfClient.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AccessibleModules { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // 显示用的格式化属性
        public string FormattedCreatedAt => CreatedAt.ToString("yyyy-MM-dd HH:mm");
        public string AccessibleModulesDisplay => !string.IsNullOrEmpty(AccessibleModules) ? AccessibleModules : "无权限";
    }
}
