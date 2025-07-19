namespace Sellsys.Domain.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public required string Name { get; set; } // e.g., "CustomerManagement.View", "Order.Create"
        public string? Description { get; set; }
    }
}