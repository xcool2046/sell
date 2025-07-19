using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sellsys.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 型号规格
        /// </summary>
        [StringLength(100)]
        public string? Specification { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        [StringLength(20)]
        public string? Unit { get; set; }

        /// <summary>
        /// 产品定价
        /// </summary>
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ListPrice { get; set; }

        /// <summary>
        /// 最低售价
        /// </summary>
        [Column(TypeName = "decimal(10, 2)")]
        public decimal MinPrice { get; set; }

        /// <summary>
        /// 销售提成
        /// </summary>
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? SalesCommission { get; set; }

        /// <summary>
        /// 主管提成
        /// </summary>
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? SupervisorCommission { get; set; }

        /// <summary>
        /// 经理提成
        /// </summary>
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? ManagerCommission { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}