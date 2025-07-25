using Sellsys.Application.DTOs.Customers;
using Sellsys.CrossCutting.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sellsys.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<ApiResponse<List<CustomerDto>>> GetAllCustomersAsync();
        Task<ApiResponse<List<CustomerDto>>> GetCustomersWithPermissionAsync(int? userId = null);
        Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(int id);
        Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CustomerUpsertDto customerDto);
        Task<ApiResponse> UpdateCustomerAsync(int id, CustomerUpsertDto customerDto);
        Task<ApiResponse> DeleteCustomerAsync(int id);
    }
}