using System.ComponentModel.DataAnnotations;

namespace Sellsys.Domain.Entities
{
    public class Department
    {
        public int Id { get; set; }
        
        /// <summary>
        /// 部门名称
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<DepartmentGroup> Groups { get; set; } = new List<DepartmentGroup>();
    }
}
