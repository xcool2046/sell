namespace Sellsys.WpfClient.Models
{
    public class DepartmentGroup
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Department? Department { get; set; }

        // Display properties
        public string FormattedCreatedAt => CreatedAt.ToString("yyyy-MM-dd HH:mm");
        public string DepartmentName => Department?.Name ?? "未知部门";
    }
}
