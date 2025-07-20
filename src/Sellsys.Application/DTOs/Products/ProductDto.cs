namespace Sellsys.Application.DTOs.Products
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Specification { get; set; }
        public string? Unit { get; set; }
        public decimal ListPrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal? SalesCommission { get; set; }
        public decimal? SupervisorCommission { get; set; }
        public decimal? ManagerCommission { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}