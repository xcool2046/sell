using System.ComponentModel.DataAnnotations;
using Sellsys.Domain.Common;

namespace Sellsys.Domain.Entities
{
    public class SalesFollowUpLog
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        
        public int? ContactId { get; set; }
        public Contact? Contact { get; set; }
        
        public string? Summary { get; set; }
        
        /// <summary>
        /// 客户意向 (高,中,低,无)
        /// </summary>
        [StringLength(50)]
        public string? CustomerIntention { get; set; }
        
        public DateTime? NextFollowUpDate { get; set; }
        
        public int? SalesPersonId { get; set; }
        public Employee? SalesPerson { get; set; }
        
        public DateTime CreatedAt { get; set; } = TimeHelper.GetBeijingTime();
    }
}
