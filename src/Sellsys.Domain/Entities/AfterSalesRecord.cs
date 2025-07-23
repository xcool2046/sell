using System.ComponentModel.DataAnnotations;
using Sellsys.Domain.Common;

namespace Sellsys.Domain.Entities
{
    public class AfterSalesRecord
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        
        public int? ContactId { get; set; }
        public Contact? Contact { get; set; }
        
        /// <summary>
        /// 客户反馈
        /// </summary>
        public string? CustomerFeedback { get; set; }
        
        /// <summary>
        /// 售后回复
        /// </summary>
        public string? OurReply { get; set; }
        
        /// <summary>
        /// 处理状态 (待处理,处理中,处理完成)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "待处理";
        
        /// <summary>
        /// 处理时间
        /// </summary>
        public DateTime? ProcessedDate { get; set; }
        
        public int? SupportPersonId { get; set; }
        public Employee? SupportPerson { get; set; }
        
        public DateTime CreatedAt { get; set; } = TimeHelper.GetBeijingTime();
        public DateTime UpdatedAt { get; set; } = TimeHelper.GetBeijingTime();
    }
}
