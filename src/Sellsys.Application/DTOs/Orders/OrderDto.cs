namespace Sellsys.Application.DTOs.Orders
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int SalesPersonId { get; set; }
        public string SalesPersonName { get; set; } = string.Empty;
        public DateTime? PaymentReceivedDate { get; set; }
        public decimal? SalesCommissionAmount { get; set; }
        public decimal? SupervisorCommissionAmount { get; set; }
        public decimal? ManagerCommissionAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        
        // Calculated properties
        public decimal TotalAmount => OrderItems.Sum(item => item.TotalAmount);
    }
}
