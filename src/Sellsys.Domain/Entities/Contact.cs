using System.ComponentModel.DataAnnotations;

namespace Sellsys.Domain.Entities
{
    public class Contact
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? Phone { get; set; }
        
        public bool IsPrimary { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<SalesFollowUpLog> SalesFollowUpLogs { get; set; } = new List<SalesFollowUpLog>();
        public ICollection<AfterSalesRecord> AfterSalesRecords { get; set; } = new List<AfterSalesRecord>();
    }
}
