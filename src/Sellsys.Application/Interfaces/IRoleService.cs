using Sellsys.Application.DTOs.Roles;
using Sellsys.CrossCutting.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sellsys.Application.Interfaces
{
    public interface IRoleService
    {
        Task<ApiResponse<List<RoleDto>>> GetAllRolesAsync();
        Task<ApiResponse<RoleDto>> GetRoleByIdAsync(int id);
        Task<ApiResponse<RoleDto>> CreateRoleAsync(RoleUpsertDto roleDto);
        Task<ApiResponse> UpdateRoleAsync(int id, RoleUpsertDto roleDto);
        Task<ApiResponse> DeleteRoleAsync(int id);
    }
}