namespace Sellsys.WpfClient.Models
{
    public class SalesFollowUpLog
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

        // 显示用的格式化属性
        public string FormattedCreatedAt => CreatedAt.ToString("yyyy-MM-dd HH:mm");
        public string FormattedNextFollowUpDate => NextFollowUpDate?.ToString("yyyy-MM-dd") ?? "未设置";
        public string ContactInfo => !string.IsNullOrEmpty(ContactName) ? ContactName : "无指定联系人";
        public string IntentionDisplay => !string.IsNullOrEmpty(CustomerIntention) ? CustomerIntention : "未记录";
    }
}
