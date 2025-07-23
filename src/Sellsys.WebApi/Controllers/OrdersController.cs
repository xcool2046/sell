using Microsoft.AspNetCore.Mvc;
using Sellsys.Application.DTOs.Orders;
using Sellsys.Application.Interfaces;

namespace Sellsys.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var response = await _orderService.GetAllOrdersAsync();
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var response = await _orderService.GetOrderByIdAsync(id);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderUpsertDto orderDto)
        {
            var response = await _orderService.CreateOrderAsync(orderDto);
            if (response.IsSuccess && response.Data != null)
            {
                return CreatedAtAction(nameof(GetOrderById), new { id = response.Data.Id }, response);
            }
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchOrders(
            [FromQuery] string? customerName = null,
            [FromQuery] string? productName = null,
            [FromQuery] DateTime? effectiveDateFrom = null,
            [FromQuery] DateTime? effectiveDateTo = null,
            [FromQuery] DateTime? expiryDateFrom = null,
            [FromQuery] DateTime? expiryDateTo = null,
            [FromQuery] DateTime? createdDateFrom = null,
            [FromQuery] DateTime? createdDateTo = null,
            [FromQuery] string? status = null,
            [FromQuery] int? salesPersonId = null)
        {
            var response = await _orderService.SearchOrdersAsync(
                customerName, productName, effectiveDateFrom, effectiveDateTo,
                expiryDateFrom, expiryDateTo, createdDateFrom, createdDateTo,
                status, salesPersonId);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetOrderSummary([FromQuery] string? orderIds = null)
        {
            List<int>? orderIdList = null;
            if (!string.IsNullOrWhiteSpace(orderIds))
            {
                orderIdList = orderIds.Split(',')
                    .Where(id => int.TryParse(id.Trim(), out _))
                    .Select(id => int.Parse(id.Trim()))
                    .ToList();
            }

            var response = await _orderService.GetOrderSummaryAsync(orderIdList);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var response = await _orderService.DeleteOrderAsync(id);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }
    }
}
