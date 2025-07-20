using System.ComponentModel.DataAnnotations;

namespace Sellsys.Application.DTOs.Customers
{
    public class ContactUpsertDto
    {
        public int? Id { get; set; } // For updates
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Phone { get; set; }

        public bool IsPrimary { get; set; } = false;
    }
}
