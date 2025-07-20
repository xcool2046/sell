using Sellsys.Application.DTOs.Employees;
using Sellsys.CrossCutting.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sellsys.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<ApiResponse<List<EmployeeDto>>> GetAllEmployeesAsync();
        Task<ApiResponse<List<EmployeeDto>>> GetEmployeesByDepartmentNameAsync(string departmentName);
        Task<ApiResponse<EmployeeDto>> GetEmployeeByIdAsync(int id);
        Task<ApiResponse<EmployeeDto>> CreateEmployeeAsync(EmployeeUpsertDto employeeDto);
        Task<ApiResponse> UpdateEmployeeAsync(int id, EmployeeUpsertDto employeeDto);
        Task<ApiResponse> DeleteEmployeeAsync(int id);
    }
}