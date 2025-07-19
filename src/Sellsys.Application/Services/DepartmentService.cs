using Microsoft.EntityFrameworkCore;
using Sellsys.CrossCutting.Common;
using Sellsys.Domain.Entities;
using Sellsys.Infrastructure.Data;
using System.Net;

namespace Sellsys.Application.Services
{
    public class DepartmentService
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

        public async Task<ApiResponse> DeleteDepartmentAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return ApiResponse.Fail("部门不存在", HttpStatusCode.NotFound);
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return ApiResponse.Success();
        }
    }
}
