using System.ComponentModel.DataAnnotations;

namespace Sellsys.Domain.Entities
{
    public class DepartmentGroup
    {
        public int Id { get; set; }
        
        /// <summary>
        /// 分组名称
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 所属部门ID
        /// </summary>
        [Required]
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
