using Sellsys.Application.DTOs.Departments;
using Sellsys.CrossCutting.Common;
using Sellsys.Domain.Entities;

namespace Sellsys.Application.Interfaces
{
    public interface IDepartmentGroupService
    {
        Task<ApiResponse<List<DepartmentGroup>>> GetAllGroupsAsync();
        Task<ApiResponse<List<DepartmentGroup>>> GetGroupsByDepartmentIdAsync(int departmentId);
        Task<ApiResponse<DepartmentGroup>> GetGroupByIdAsync(int id);
        Task<ApiResponse<DepartmentGroup>> CreateGroupAsync(int departmentId, string name);
        Task<ApiResponse> UpdateGroupAsync(int id, DepartmentGroupUpsertDto groupDto);
        Task<ApiResponse> DeleteGroupAsync(int id);
    }
}
