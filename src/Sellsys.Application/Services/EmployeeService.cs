using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Sellsys.Application.DTOs.Employees;
using Sellsys.Application.Interfaces;
using Sellsys.CrossCutting.Common;
using Sellsys.Domain.Entities;
using Sellsys.Domain.Common;
using Sellsys.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sellsys.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly SellsysDbContext _context;

        public EmployeeService(SellsysDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<EmployeeDto>> CreateEmployeeAsync(EmployeeUpsertDto employeeDto)
        {
            if (string.IsNullOrEmpty(employeeDto.Password))
            {
                return new ApiResponse<EmployeeDto> { IsSuccess = false, Message = "Password is required for new employees.", StatusCode = HttpStatusCode.BadRequest };
            }

            var existingUser = await _context.Employees.FirstOrDefaultAsync(e => e.LoginUsername == employeeDto.LoginUsername);
            if (existingUser != null)
            {
                return new ApiResponse<EmployeeDto> { IsSuccess = false, Message = "Login username already exists.", StatusCode = HttpStatusCode.BadRequest };
            }

            var employee = new Employee
            {
                Name = employeeDto.Name,
                LoginUsername = employeeDto.LoginUsername,
                Phone = employeeDto.Phone,
                BranchAccount = employeeDto.BranchAccount,
                GroupId = employeeDto.GroupId,
                RoleId = employeeDto.RoleId,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(employeeDto.Password),
                CreatedAt = TimeHelper.GetBeijingTime()
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // 重新查询以获取关联数据
            var createdEmployee = await _context.Employees
                .Include(e => e.Group)
                    .ThenInclude(g => g!.Department)
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Id == employee.Id);

            var resultDto = new EmployeeDto
            {
                Id = createdEmployee!.Id,
                Name = createdEmployee.Name,
                LoginUsername = createdEmployee.LoginUsername,
                Phone = createdEmployee.Phone,
                BranchAccount = createdEmployee.BranchAccount,
                GroupId = createdEmployee.GroupId,
                GroupName = createdEmployee.Group?.Name,
                DepartmentName = createdEmployee.Group?.Department?.Name,
                RoleId = createdEmployee.RoleId,
                RoleName = createdEmployee.Role?.Name,
                CreatedAt = createdEmployee.CreatedAt
            };

            return ApiResponse<EmployeeDto>.Success(resultDto);
        }

        public async Task<ApiResponse> DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return ApiResponse.Fail("Employee not found.", HttpStatusCode.NotFound);
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return ApiResponse.Success();
        }

        public async Task<ApiResponse<List<EmployeeDto>>> GetAllEmployeesAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.Group)
                    .ThenInclude(g => g!.Department)
                .Include(e => e.Role)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    LoginUsername = e.LoginUsername,
                    Phone = e.Phone,
                    BranchAccount = e.BranchAccount,
                    GroupId = e.GroupId,
                    GroupName = e.Group != null ? e.Group.Name : null,
                    DepartmentName = e.Group != null && e.Group.Department != null ? e.Group.Department.Name : null,
                    RoleId = e.RoleId,
                    RoleName = e.Role != null ? e.Role.Name : null,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<List<EmployeeDto>>.Success(employees);
        }

        public async Task<ApiResponse<List<EmployeeDto>>> GetEmployeesByDepartmentNameAsync(string departmentName)
        {
            var employees = await _context.Employees
                .Include(e => e.Group)
                    .ThenInclude(g => g!.Department)
                .Include(e => e.Role)
                .Where(e => e.Group != null && e.Group.Department != null && e.Group.Department.Name == departmentName)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    LoginUsername = e.LoginUsername,
                    Phone = e.Phone,
                    BranchAccount = e.BranchAccount,
                    GroupId = e.GroupId,
                    GroupName = e.Group != null ? e.Group.Name : null,
                    DepartmentName = e.Group != null && e.Group.Department != null ? e.Group.Department.Name : null,
                    RoleId = e.RoleId,
                    RoleName = e.Role != null ? e.Role.Name : null,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<List<EmployeeDto>>.Success(employees);
        }

        public async Task<ApiResponse<List<EmployeeDto>>> GetEmployeesByGroupIdAsync(int groupId)
        {
            var employees = await _context.Employees
                .Include(e => e.Group)
                    .ThenInclude(g => g!.Department)
                .Include(e => e.Role)
                .Where(e => e.GroupId == groupId)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    LoginUsername = e.LoginUsername,
                    Phone = e.Phone,
                    BranchAccount = e.BranchAccount,
                    GroupId = e.GroupId,
                    GroupName = e.Group != null ? e.Group.Name : null,
                    DepartmentName = e.Group != null && e.Group.Department != null ? e.Group.Department.Name : null,
                    RoleId = e.RoleId,
                    RoleName = e.Role != null ? e.Role.Name : null,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<List<EmployeeDto>>.Success(employees);
        }

        public async Task<ApiResponse<EmployeeDto>> GetEmployeeByIdAsync(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Group)
                    .ThenInclude(g => g!.Department)
                .Include(e => e.Role)
                .Where(e => e.Id == id)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    LoginUsername = e.LoginUsername,
                    Phone = e.Phone,
                    BranchAccount = e.BranchAccount,
                    GroupId = e.GroupId,
                    GroupName = e.Group != null ? e.Group.Name : null,
                    DepartmentName = e.Group != null && e.Group.Department != null ? e.Group.Department.Name : null,
                    RoleId = e.RoleId,
                    RoleName = e.Role != null ? e.Role.Name : null,
                    CreatedAt = e.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (employee == null)
            {
                return new ApiResponse<EmployeeDto> { IsSuccess = false, Message = "Employee not found", StatusCode = HttpStatusCode.NotFound };
            }

            return ApiResponse<EmployeeDto>.Success(employee);
        }

        public async Task<ApiResponse> UpdateEmployeeAsync(int id, EmployeeUpsertDto employeeDto)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return ApiResponse.Fail("Employee not found.", HttpStatusCode.NotFound);
            }

            if (employee.LoginUsername != employeeDto.LoginUsername)
            {
                 var existingUser = await _context.Employees.FirstOrDefaultAsync(e => e.LoginUsername == employeeDto.LoginUsername);
                 if (existingUser != null)
                 {
                    return ApiResponse.Fail("Login username already exists.", HttpStatusCode.BadRequest);
                 }
            }

            employee.Name = employeeDto.Name;
            employee.LoginUsername = employeeDto.LoginUsername;
            employee.Phone = employeeDto.Phone;
            employee.BranchAccount = employeeDto.BranchAccount;
            employee.GroupId = employeeDto.GroupId;
            employee.RoleId = employeeDto.RoleId;

            if (!string.IsNullOrEmpty(employeeDto.Password))
            {
                employee.HashedPassword = BCrypt.Net.BCrypt.HashPassword(employeeDto.Password);
            }

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();

            return ApiResponse.Success();
        }
    }
}