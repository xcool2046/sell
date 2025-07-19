using Microsoft.AspNetCore.Mvc;
using Sellsys.Application.DTOs.Sales;
using Sellsys.Application.Interfaces;
using System.Threading.Tasks;

namespace Sellsys.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrdersController : ControllerBase
    {
        private readonly ISalesOrderService _salesOrderService;

        public SalesOrdersController(ISalesOrderService salesOrderService)
        {
            _salesOrderService = salesOrderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesOrders()
        {
            var response = await _salesOrderService.GetAllSalesOrdersAsync();
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSalesOrder(int id)
        {
            var response = await _salesOrderService.GetSalesOrderByIdAsync(id);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpPost]
        public async Task<IActionResult> CreateSalesOrder([FromBody] SalesOrderUpsertDto salesOrderDto)
        {
            var response = await _salesOrderService.CreateSalesOrderAsync(salesOrderDto);
            if (response.IsSuccess && response.Data != null)
            {
                return CreatedAtAction(nameof(GetSalesOrder), new { id = response.Data.Id }, response);
            }
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSalesOrder(int id)
        {
            var response = await _salesOrderService.DeleteSalesOrderAsync(id);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }
    }
}