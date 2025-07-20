using System.ComponentModel.DataAnnotations;

namespace Sellsys.Application.DTOs.Employees
{
    public class EmployeeUpsertDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LoginUsername { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? BranchAccount { get; set; }

        public int? GroupId { get; set; }
        public int? RoleId { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }
    }
}