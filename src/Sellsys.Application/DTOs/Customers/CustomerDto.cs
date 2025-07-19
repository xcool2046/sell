namespace Sellsys.Application.DTOs.Customers
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? Remarks { get; set; }
        public string? IndustryTypes { get; set; }
        public int? SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }
        public int? SupportPersonId { get; set; }
        public string? SupportPersonName { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public List<ContactDto> Contacts { get; set; } = new List<ContactDto>();
    }
}