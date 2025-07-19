using System.ComponentModel.DataAnnotations;

namespace Sellsys.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }

        /// <summary>
        /// 岗位职务名称
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 可访问的模块名列表,逗号分隔
        /// </summary>
        public string? AccessibleModules { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}