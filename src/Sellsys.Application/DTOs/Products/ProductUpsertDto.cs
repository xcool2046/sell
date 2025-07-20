using System.ComponentModel.DataAnnotations;

namespace Sellsys.Application.DTOs.Products
{
    public class ProductUpsertDto
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Specification { get; set; }

        [StringLength(20)]
        public string? Unit { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal ListPrice { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal MinPrice { get; set; }

        [Range(0, 1000000)]
        public decimal? SalesCommission { get; set; }

        [Range(0, 1000000)]
        public decimal? SupervisorCommission { get; set; }

        [Range(0, 1000000)]
        public decimal? ManagerCommission { get; set; }
    }
}