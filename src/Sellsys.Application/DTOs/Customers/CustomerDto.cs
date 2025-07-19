namespace Sellsys.Application.DTOs.Customers
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? ContactPerson { get; set; }
        public string? PhoneNumber { get; set; }
        
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}