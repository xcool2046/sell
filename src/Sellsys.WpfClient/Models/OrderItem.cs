namespace Sellsys.WpfClient.Models
{
    public class OrderItem
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

        // 显示属性
        public string ProductInfo => !string.IsNullOrEmpty(ProductSpecification) 
            ? $"{ProductName} ({ProductSpecification})" 
            : ProductName;
        
        public string UnitDisplay => !string.IsNullOrEmpty(ProductUnit) ? ProductUnit : "件";
        public string FormattedActualPrice => ActualPrice.ToString("C");
        public string FormattedTotalAmount => TotalAmount.ToString("C");
    }
}
