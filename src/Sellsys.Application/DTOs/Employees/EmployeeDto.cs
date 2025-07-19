using System;

namespace Sellsys.Application.DTOs.Employees
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Department { get; set; }
        public string? Group { get; set; }
        public required string Position { get; set; }
        public required string PhoneNumber { get; set; }
        public required string BranchAccount { get; set; }
        public required string LoginAccount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}