using Sellsys.Application.DTOs.Sales;
using Sellsys.CrossCutting.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sellsys.Application.Interfaces
{
    public interface ISalesOrderService
    {
        Task<ApiResponse<List<SalesOrderDto>>> GetAllSalesOrdersAsync();
        Task<ApiResponse<SalesOrderDto>> GetSalesOrderByIdAsync(int id);
        Task<ApiResponse<SalesOrderDto>> CreateSalesOrderAsync(SalesOrderUpsertDto salesOrderDto);
        Task<ApiResponse> DeleteSalesOrderAsync(int id);
    }
}