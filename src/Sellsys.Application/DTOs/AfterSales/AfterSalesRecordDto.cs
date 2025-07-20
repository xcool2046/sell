namespace Sellsys.Application.DTOs.AfterSales
{
    public class AfterSalesRecordDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerProvince { get; set; }
        public string? CustomerCity { get; set; }
        public int? ContactId { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? CustomerFeedback { get; set; }
        public string? OurReply { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ProcessedDate { get; set; }
        public int? SupportPersonId { get; set; }
        public string? SupportPersonName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
