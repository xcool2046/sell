using System.ComponentModel.DataAnnotations;

namespace Sellsys.Application.DTOs.AfterSales
{
    public class AfterSalesRecordUpsertDto
    {
        [Required(ErrorMessage = "客户ID不能为空")]
        public int CustomerId { get; set; }
        
        public int? ContactId { get; set; }
        
        [StringLength(2000, ErrorMessage = "客户反馈内容不能超过2000个字符")]
        public string? CustomerFeedback { get; set; }
        
        [StringLength(2000, ErrorMessage = "回复内容不能超过2000个字符")]
        public string? OurReply { get; set; }
        
        [Required(ErrorMessage = "处理状态不能为空")]
        [StringLength(50, ErrorMessage = "处理状态长度不能超过50个字符")]
        public string Status { get; set; } = "待处理";
        
        public DateTime? ProcessedDate { get; set; }
        
        public int? SupportPersonId { get; set; }
    }
}
