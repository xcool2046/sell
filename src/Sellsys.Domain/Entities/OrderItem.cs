using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sellsys.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        
        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        /// <summary>
        /// 实际售价
        /// </summary>
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ActualPrice { get; set; }
        
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// 订单项总金额
        /// </summary>
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
