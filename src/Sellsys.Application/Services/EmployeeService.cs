using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Sellsys.Application.DTOs.Employees;
using Sellsys.Application.Interfaces;
using Sellsys.CrossCutting.Common;
using Sellsys.Domain.Entities;
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

            var existingUser = await _context.Employees.FirstOrDefaultAsync(e => e.LoginAccount == employeeDto.LoginAccount);
            if (existingUser != null)
            {
                return new ApiResponse<EmployeeDto> { IsSuccess = false, Message = "Login account already exists.", StatusCode = HttpStatusCode.BadRequest };
            }

            var employee = new Employee
            {
                Name = employeeDto.Name,
                Department = employeeDto.Department,
                Group = employeeDto.Group,
                Position = employeeDto.Position,
                PhoneNumber = employeeDto.PhoneNumber,
                BranchAccount = employeeDto.BranchAccount,
                LoginAccount = employeeDto.LoginAccount,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(employeeDto.Password)
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var resultDto = new EmployeeDto
            {
                Id = employee.Id,
                Name = employee.Name,
                Department = employee.Department,
                Group = employee.Group,
                Position = employee.Position,
                PhoneNumber = employee.PhoneNumber,
                BranchAccount = employee.BranchAccount,
                LoginAccount = employee.LoginAccount,
                CreatedAt = employee.CreatedAt
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
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Department = e.Department,
                    Group = e.Group,
                    Position = e.Position,
                    PhoneNumber = e.PhoneNumber,
                    BranchAccount = e.BranchAccount,
                    LoginAccount = e.LoginAccount,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<List<EmployeeDto>>.Success(employees);
        }

        public async Task<ApiResponse<EmployeeDto>> GetEmployeeByIdAsync(int id)
        {
            var employee = await _context.Employees
                .Where(e => e.Id == id)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Department = e.Department,
                    Group = e.Group,
                    Position = e.Position,
                    PhoneNumber = e.PhoneNumber,
                    BranchAccount = e.BranchAccount,
                    LoginAccount = e.LoginAccount,
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

            if (employee.LoginAccount != employeeDto.LoginAccount)
            {
                 var existingUser = await _context.Employees.FirstOrDefaultAsync(e => e.LoginAccount == employeeDto.LoginAccount);
                 if (existingUser != null)
                 {
                    return ApiResponse.Fail("Login account already exists.", HttpStatusCode.BadRequest);
                 }
            }

            employee.Name = employeeDto.Name;
            employee.Department = employeeDto.Department;
            employee.Group = employeeDto.Group;
            employee.Position = employeeDto.Position;
            employee.PhoneNumber = employeeDto.PhoneNumber;
            employee.BranchAccount = employeeDto.BranchAccount;
            employee.LoginAccount = employeeDto.LoginAccount;

            if (!string.IsNullOrEmpty(employeeDto.Password))
            {
                employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(employeeDto.Password);
            }

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();

            return ApiResponse.Success();
        }
    }
}