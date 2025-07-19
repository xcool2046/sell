namespace Sellsys.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public required string Name { get; set; } // e.g., "销售经理", "客服专员"
        public required string Department { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for permissions
        public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
    }
}