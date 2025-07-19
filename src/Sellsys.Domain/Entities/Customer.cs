namespace Sellsys.Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? ContactPerson { get; set; }
        public required string PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Industry { get; set; } // 行业
        public string? Source { get; set; } // 客户来源
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key for the assigned employee
        public int? AssignedToEmployeeId { get; set; }
        public Employee? AssignedToEmployee { get; set; }
    }
}