using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sellsys.Domain.Common;

namespace Sellsys.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;
        
        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "待收款";
        
        [Required]
        public int SalesPersonId { get; set; }
        public Employee SalesPerson { get; set; } = null!;
        
        /// <summary>
        /// 到账日期
        /// </summary>
        public DateTime? PaymentReceivedDate { get; set; }
        
        /// <summary>
        /// 销售提成快照
        /// </summary>
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? SalesCommissionAmount { get; set; }
        
        /// <summary>
        /// 主管提成快照
        /// </summary>
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? SupervisorCommissionAmount { get; set; }
        
        /// <summary>
        /// 经理提成快照
        /// </summary>
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? ManagerCommissionAmount { get; set; }
        
        public DateTime CreatedAt { get; set; } = TimeHelper.GetBeijingTime();
        public DateTime UpdatedAt { get; set; } = TimeHelper.GetBeijingTime();
        
        // Navigation properties
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        
        // Calculated properties
        [NotMapped]
        public decimal TotalAmount => OrderItems.Sum(item => item.TotalAmount);
    }
}
