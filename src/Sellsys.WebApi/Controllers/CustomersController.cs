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
        public async Task<ActionResult<ApiResponse<List<CustomerDto>>>> GetCustomers()
        {
            var response = await _customerService.GetAllCustomersAsync();
            return Ok(response);
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
        public async Task<ActionResult<ApiResponse<CustomerDto>>> CreateCustomer(CustomerUpsertDto customerDto)
        {
            var response = await _customerService.CreateCustomerAsync(customerDto);
            if (response.Data == null)
            {
                return BadRequest(response);
            }
            return CreatedAtAction(nameof(GetCustomer), new { id = response.Data.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, CustomerUpsertDto customerDto)
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