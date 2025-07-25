using Microsoft.AspNetCore.Mvc;
using Sellsys.Application.DTOs.Customers;
using Sellsys.Application.Interfaces;
using Sellsys.CrossCutting.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sellsys.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CustomerDto>>>> GetCustomers([FromQuery] int? userId = null)
        {
            // 如果传递了userId参数，使用权限控制的方法
            if (userId.HasValue)
            {
                var response = await _customerService.GetCustomersWithPermissionAsync(userId.Value);
                return Ok(response);
            }

            // 否则返回所有客户（管理员权限）
            var allCustomersResponse = await _customerService.GetAllCustomersAsync();
            return Ok(allCustomersResponse);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomer(int id)
        {
            var response = await _customerService.GetCustomerByIdAsync(id);
            if (!response.IsSuccess)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> CreateCustomer([FromBody] CustomerUpsertDto customerDto)
        {
            var response = await _customerService.CreateCustomerAsync(customerDto);
            if (response.Data == null)
            {
                return BadRequest(response);
            }
            return CreatedAtAction(nameof(GetCustomer), new { id = response.Data.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerUpsertDto customerDto)
        {
            var response = await _customerService.UpdateCustomerAsync(id, customerDto);
            if (!response.IsSuccess)
            {
                return NotFound(response);
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var response = await _customerService.DeleteCustomerAsync(id);
            if (!response.IsSuccess)
            {
                return NotFound(response);
            }
            return NoContent();
        }
    }
}