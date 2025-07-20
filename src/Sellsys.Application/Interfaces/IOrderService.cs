using Sellsys.Application.DTOs.Orders;
using Sellsys.CrossCutting.Common;

namespace Sellsys.Application.Interfaces
{
    public interface IOrderService
    {
        Task<ApiResponse<OrderDto>> CreateOrderAsync(OrderUpsertDto orderDto);
        Task<ApiResponse<List<OrderDto>>> GetAllOrdersAsync();
        Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int id);
        Task<ApiResponse<List<OrderDto>>> SearchOrdersAsync(
            string? customerName = null,
            string? productName = null,
            DateTime? effectiveDateFrom = null,
            DateTime? effectiveDateTo = null,
            DateTime? expiryDateFrom = null,
            DateTime? expiryDateTo = null,
            DateTime? createdDateFrom = null,
            DateTime? createdDateTo = null,
            string? status = null,
            int? salesPersonId = null);
        Task<ApiResponse<OrderSummaryDto>> GetOrderSummaryAsync(List<int>? orderIds = null);
        Task<ApiResponse> DeleteOrderAsync(int id);
    }
}
