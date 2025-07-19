using System.ComponentModel.DataAnnotations;

namespace Sellsys.Application.DTOs.Customers
{
    public class CustomerUpsertDto
    {
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [StringLength(50)]
        public string? ContactPerson { get; set; }

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }
    }
}