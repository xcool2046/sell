using System.ComponentModel.DataAnnotations;

namespace Sellsys.Domain.Entities
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LoginUsername { get; set; } = string.Empty;

        [StringLength(255)]
        public string? HashedPassword { get; set; }

        [StringLength(50)]
        public string? Phone { get; set; }

        /// <summary>
        /// 网点账号
        /// </summary>
        [StringLength(100)]
        public string? BranchAccount { get; set; }

        /// <summary>
        /// 所属分组ID
        /// </summary>
        public int? GroupId { get; set; }
        public DepartmentGroup? Group { get; set; }

        /// <summary>
        /// 岗位职务ID (角色)
        /// </summary>
        public int? RoleId { get; set; }
        public Role? Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Customer> SalesCustomers { get; set; } = new List<Customer>();
        public ICollection<Customer> SupportCustomers { get; set; } = new List<Customer>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<SalesFollowUpLog> SalesFollowUpLogs { get; set; } = new List<SalesFollowUpLog>();
        public ICollection<AfterSalesRecord> AfterSalesRecords { get; set; } = new List<AfterSalesRecord>();
    }
}