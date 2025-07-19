using Sellsys.CrossCutting.Common;
using Sellsys.Domain.Entities;

namespace Sellsys.Application.Interfaces
{
    public interface IDepartmentService
    {
        Task<ApiResponse<List<Department>>> GetAllDepartmentsAsync();
        Task<ApiResponse<Department>> GetDepartmentByIdAsync(int id);
        Task<ApiResponse<Department>> CreateDepartmentAsync(string name);
        Task<ApiResponse> UpdateDepartmentAsync(int id, string name);
        Task<ApiResponse> DeleteDepartmentAsync(int id);
    }
}
