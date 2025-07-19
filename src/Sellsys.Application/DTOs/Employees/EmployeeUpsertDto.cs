using System.ComponentModel.DataAnnotations;

namespace Sellsys.Application.DTOs.Employees
{
    public class EmployeeUpsertDto
    {
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [StringLength(100)]
        public required string Department { get; set; }

        [StringLength(100)]
        public string? Group { get; set; }
        
        [Required]
        [StringLength(50)]
        public required string Position { get; set; }

        [Required]
        [StringLength(20)]
        public required string PhoneNumber { get; set; }

        [Required]
        [StringLength(50)]
        public required string BranchAccount { get; set; }

        [Required]
        [StringLength(50)]
        public required string LoginAccount { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }
    }
}