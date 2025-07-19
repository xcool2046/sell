using Microsoft.EntityFrameworkCore;
using Sellsys.Application.Interfaces;
using Sellsys.CrossCutting.Common;
using Sellsys.Domain.Entities;
using Sellsys.Infrastructure.Data;
using System.Net;

namespace Sellsys.Application.Services
{
    public class DepartmentGroupService : IDepartmentGroupService
    {
        private readonly SellsysDbContext _context;

        public DepartmentGroupService(SellsysDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<DepartmentGroup>>> GetAllGroupsAsync()
        {
            var groups = await _context.DepartmentGroups
                .Include(g => g.Department)
                .ToListAsync();

            return ApiResponse<List<DepartmentGroup>>.Success(groups);
        }

        public async Task<ApiResponse<List<DepartmentGroup>>> GetGroupsByDepartmentIdAsync(int departmentId)
        {
            var groups = await _context.DepartmentGroups
                .Include(g => g.Department)
                .Where(g => g.DepartmentId == departmentId)
                .ToListAsync();

            return ApiResponse<List<DepartmentGroup>>.Success(groups);
        }

        public async Task<ApiResponse<DepartmentGroup>> GetGroupByIdAsync(int id)
        {
            var group = await _context.DepartmentGroups
                .Include(g => g.Department)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return new ApiResponse<DepartmentGroup> { IsSuccess = false, Message = "分组不存在", StatusCode = HttpStatusCode.NotFound };
            }

            return ApiResponse<DepartmentGroup>.Success(group);
        }

        public async Task<ApiResponse<DepartmentGroup>> CreateGroupAsync(int departmentId, string name)
        {
            // 检查部门是否存在
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null)
            {
                return new ApiResponse<DepartmentGroup> { IsSuccess = false, Message = "部门不存在", StatusCode = HttpStatusCode.BadRequest };
            }

            var group = new DepartmentGroup
            {
                Name = name,
                DepartmentId = departmentId
            };

            _context.DepartmentGroups.Add(group);
            await _context.SaveChangesAsync();

            // 重新查询以获取关联数据
            var createdGroup = await _context.DepartmentGroups
                .Include(g => g.Department)
                .FirstOrDefaultAsync(g => g.Id == group.Id);

            return ApiResponse<DepartmentGroup>.Success(createdGroup!);
        }

        public async Task<ApiResponse> UpdateGroupAsync(int id, string name)
        {
            var group = await _context.DepartmentGroups.FindAsync(id);
            if (group == null)
            {
                return ApiResponse.Fail("分组不存在", HttpStatusCode.NotFound);
            }

            group.Name = name;
            _context.DepartmentGroups.Update(group);
            await _context.SaveChangesAsync();

            return ApiResponse.Success();
        }

        public async Task<ApiResponse> DeleteGroupAsync(int id)
        {
            var group = await _context.DepartmentGroups.FindAsync(id);
            if (group == null)
            {
                return ApiResponse.Fail("分组不存在", HttpStatusCode.NotFound);
            }

            _context.DepartmentGroups.Remove(group);
            await _context.SaveChangesAsync();
            return ApiResponse.Success();
        }
    }
}
