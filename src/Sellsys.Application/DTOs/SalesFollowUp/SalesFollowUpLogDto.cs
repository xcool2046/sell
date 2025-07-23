namespace Sellsys.Application.DTOs.SalesFollowUp
{
    public class SalesFollowUpLogDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int? ContactId { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? Summary { get; set; }
        public string? CustomerIntention { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
        public int? SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
