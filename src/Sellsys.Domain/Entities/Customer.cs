using System.ComponentModel.DataAnnotations;

namespace Sellsys.Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Province { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        public string? Remarks { get; set; }

        /// <summary>
        /// 行业类别，逗号分隔
        /// </summary>
        [StringLength(255)]
        public string? IndustryTypes { get; set; }

        /// <summary>
        /// 负责人(销售)
        /// </summary>
        public int? SalesPersonId { get; set; }
        public Employee? SalesPerson { get; set; }

        /// <summary>
        /// 负责人(客服)
        /// </summary>
        public int? SupportPersonId { get; set; }
        public Employee? SupportPerson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public ICollection<SalesFollowUpLog> SalesFollowUpLogs { get; set; } = new List<SalesFollowUpLog>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<AfterSalesRecord> AfterSalesRecords { get; set; } = new List<AfterSalesRecord>();
    }
}