using System.ComponentModel.DataAnnotations;

namespace Sellsys.Application.DTOs.Orders
{
    public class OrderUpsertDto
    {
        [Required]
        public int CustomerId { get; set; }

        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "待收款";

        [Required]
        public int SalesPersonId { get; set; }

        public DateTime? PaymentReceivedDate { get; set; }
        public decimal? SalesCommissionAmount { get; set; }
        public decimal? SupervisorCommissionAmount { get; set; }
        public decimal? ManagerCommissionAmount { get; set; }

        // 订单项列表
        public List<OrderItemUpsertDto> OrderItems { get; set; } = new List<OrderItemUpsertDto>();
    }
}
