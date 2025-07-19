using System.ComponentModel.DataAnnotations;

namespace Sellsys.Application.DTOs.Roles
{
    public class RoleUpsertDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // 可访问的模块名列表
        public List<string> AccessibleModules { get; set; } = new List<string>();
    }
}