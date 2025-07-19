namespace Sellsys.Application.DTOs.Roles
{
    public class RoleDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Department { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Permissions { get; set; } = [];
    }
}