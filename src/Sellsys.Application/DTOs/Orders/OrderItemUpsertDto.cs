using System.ComponentModel.DataAnnotations;

namespace Sellsys.Application.DTOs.Orders
{
    public class OrderItemUpsertDto
    {
        public int? Id { get; set; } // For updates

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal ActualPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal TotalAmount { get; set; }
    }
}
