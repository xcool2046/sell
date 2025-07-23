namespace Sellsys.Application.DTOs.Employees
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LoginUsername { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? BranchAccount { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? DepartmentName { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}