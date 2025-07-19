namespace Sellsys.Domain.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Department { get; set; }
        public string? Group { get; set; }
        public required string Position { get; set; } // This is the Role
        public required string PhoneNumber { get; set; }
        public required string BranchAccount { get; set; }
        public required string LoginAccount { get; set; }
        public required string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}