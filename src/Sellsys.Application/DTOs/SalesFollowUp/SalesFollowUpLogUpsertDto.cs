namespace Sellsys.Application.DTOs.SalesFollowUp
{
    public class SalesFollowUpLogUpsertDto
    {
        public int CustomerId { get; set; }
        public int? ContactId { get; set; }
        public string? Summary { get; set; }
        public string? CustomerIntention { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
        public int? SalesPersonId { get; set; }
    }
}
