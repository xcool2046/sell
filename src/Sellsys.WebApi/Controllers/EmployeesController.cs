using Microsoft.AspNetCore.Mvc;
using Sellsys.Application.DTOs.Employees;
using Sellsys.Application.Interfaces;
using System.Threading.Tasks;

namespace Sellsys.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var response = await _employeeService.GetAllEmployeesAsync();
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpGet("by-department/{departmentName}")]
        public async Task<IActionResult> GetEmployeesByDepartment(string departmentName)
        {
            var response = await _employeeService.GetEmployeesByDepartmentNameAsync(departmentName);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpGet("by-group/{groupId}")]
        public async Task<IActionResult> GetEmployeesByGroup(int groupId)
        {
            var response = await _employeeService.GetEmployeesByGroupIdAsync(groupId);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var response = await _employeeService.GetEmployeeByIdAsync(id);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeUpsertDto employeeDto)
        {
            var response = await _employeeService.CreateEmployeeAsync(employeeDto);
            if (response.IsSuccess && response.Data != null)
            {
                return CreatedAtAction(nameof(GetEmployee), new { id = response.Data.Id }, response);
            }
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeUpsertDto employeeDto)
        {
            var response = await _employeeService.UpdateEmployeeAsync(id, employeeDto);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var response = await _employeeService.DeleteEmployeeAsync(id);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }
    }
}