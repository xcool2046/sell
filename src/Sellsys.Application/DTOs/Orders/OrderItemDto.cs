namespace Sellsys.Application.DTOs.Orders
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductSpecification { get; set; }
        public string? ProductUnit { get; set; }
        public decimal? ProductPrice { get; set; } // 产品原价
        public decimal ActualPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
