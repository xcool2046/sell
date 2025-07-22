using Microsoft.EntityFrameworkCore;
using Sellsys.Application.Interfaces;
using Sellsys.Application.DTOs.Departments;
using Sellsys.CrossCutting.Common;
using Sellsys.Domain.Entities;
using Sellsys.Infrastructure.Data;
using System.Net;

namespace Sellsys.Application.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly SellsysDbContext _context;

        public DepartmentService(SellsysDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<Department>>> GetAllDepartmentsAsync()
        {
            var departments = await _context.Departments
                .Include(d => d.Groups)
                .ToListAsync();

            return ApiResponse<List<Department>>.Success(departments);
        }

        public async Task<ApiResponse<Department>> GetDepartmentByIdAsync(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Groups)
                    .ThenInclude(g => g.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return new ApiResponse<Department> { IsSuccess = false, Message = "部门不存在", StatusCode = HttpStatusCode.NotFound };
            }

            return ApiResponse<Department>.Success(department);
        }

        public async Task<ApiResponse<Department>> CreateDepartmentAsync(string name)
        {
            var department = new Department
            {
                Name = name
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return ApiResponse<Department>.Success(department);
        }

        public async Task<ApiResponse> UpdateDepartmentAsync(int id, string name)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return ApiResponse.Fail("部门不存在", HttpStatusCode.NotFound);
            }

            department.Name = name;
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();

            return ApiResponse.Success();
        }

        public async Task<ApiResponse<DepartmentDeleteResultDto>> DeleteDepartmentAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var department = await _context.Departments
                    .Include(d => d.Groups)
                        .ThenInclude(g => g.Employees)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (department == null)
                {
                    return new ApiResponse<DepartmentDeleteResultDto>
                    {
                        IsSuccess = false,
                        Message = "部门不存在",
                        StatusCode = HttpStatusCode.NotFound
                    };
                }

                // 收集删除统计信息
                var groupCount = department.Groups.Count;
                var groupNames = department.Groups.Select(g => g.Name).ToList();
                var employeeCount = department.Groups.Sum(g => g.Employees.Count);
                var employeeNames = department.Groups.SelectMany(g => g.Employees).Select(e => e.Name).ToList();

                // 1. 删除所有员工（这会自动处理相关的权限和关联数据）
                var allEmployees = department.Groups.SelectMany(g => g.Employees).ToList();
                if (allEmployees.Any())
                {
                    _context.Employees.RemoveRange(allEmployees);
                }

                // 2. 删除所有分组
                if (department.Groups.Any())
                {
                    _context.DepartmentGroups.RemoveRange(department.Groups);
                }

                // 3. 删除部门
                _context.Departments.Remove(department);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = new DepartmentDeleteResultDto
                {
                    DepartmentId = id,
                    DepartmentName = department.Name,
                    DeletedGroupCount = groupCount,
                    DeletedGroupNames = groupNames,
                    DeletedEmployeeCount = employeeCount,
                    DeletedEmployeeNames = employeeNames
                };

                return ApiResponse<DepartmentDeleteResultDto>.Success(result,
                    $"成功删除部门 '{department.Name}'，包括 {groupCount} 个分组和 {employeeCount} 名员工");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<DepartmentDeleteResultDto>
                {
                    IsSuccess = false,
                    Message = $"删除部门失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }
    }
}
