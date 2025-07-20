using System.ComponentModel.DataAnnotations;

namespace Sellsys.Application.DTOs.Customers
{
    public class CustomerUpsertDto
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Province { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        public string? Remarks { get; set; }

        [StringLength(255)]
        public string? IndustryTypes { get; set; }

        public int? SalesPersonId { get; set; }
        public int? SupportPersonId { get; set; }

        // 联系人列表
        public List<ContactUpsertDto> Contacts { get; set; } = new List<ContactUpsertDto>();
    }
}