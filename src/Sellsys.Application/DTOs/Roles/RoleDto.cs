namespace Sellsys.Application.DTOs.Roles
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? AccessibleModules { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> ModuleList =>
            string.IsNullOrEmpty(AccessibleModules)
                ? new List<string>()
                : AccessibleModules.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}