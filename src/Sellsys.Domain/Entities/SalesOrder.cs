using System.ComponentModel.DataAnnotations.Schema;

namespace Sellsys.Domain.Entities
{
    public class SalesOrder
    {
        public int Id { get; set; }
        public required string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // e.g., Pending, Processing, Completed, Cancelled

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        // Foreign key for Customer
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        // Foreign key for Salesperson (Employee)
        public int SalespersonId { get; set; }
        public Employee Salesperson { get; set; } = null!;

        // Navigation property for Order Items
        public ICollection<SalesOrderItem> OrderItems { get; set; } = new List<SalesOrderItem>();
    }
}