using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sellsys.Application.DTOs.Roles
{
    public class RoleUpsertDto
    {
        [Required]
        [StringLength(50)]
        public required string Name { get; set; }

        [Required]
        [StringLength(50)]
        public required string Department { get; set; }

        // A list of permission NAMES to be assigned to the role
        public List<string> Permissions { get; set; } = [];
    }
}