namespace Sellsys.WpfClient.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        // 显示用的格式化属性
        public string FormattedCreatedAt => CreatedAt.ToString("yyyy-MM-dd HH:mm");
        public string DescriptionDisplay => !string.IsNullOrEmpty(Description) ? Description : "无描述";
    }
}
