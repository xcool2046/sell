using System.ComponentModel.DataAnnotations.Schema;

namespace Sellsys.Domain.Entities
{
    public class SalesOrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; } // Price at the time of sale

        // Foreign key for SalesOrder
        public int SalesOrderId { get; set; }
        public SalesOrder SalesOrder { get; set; } = null!;

        // Foreign key for Product
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}