using Microsoft.AspNetCore.Mvc;
using Sellsys.Application.DTOs.Departments;
using Sellsys.Application.Interfaces;
using Sellsys.CrossCutting.Common;

namespace Sellsys.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        /// <summary>
        /// 获取所有部门
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllDepartments()
        {
            var response = await _departmentService.GetAllDepartmentsAsync();
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 根据ID获取部门
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            var response = await _departmentService.GetDepartmentByIdAsync(id);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 创建新部门
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(ApiResponse.Fail("部门名称不能为空"));
            }

            var response = await _departmentService.CreateDepartmentAsync(name);
            if (response.IsSuccess && response.Data != null)
            {
                return CreatedAtAction(nameof(GetDepartmentById), new { id = response.Data.Id }, response);
            }
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 更新部门
        /// </summary>
        /// <param name="id"></param>
        /// <param name="departmentDto"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] DepartmentUpsertDto departmentDto)
        {
            if (departmentDto == null || string.IsNullOrWhiteSpace(departmentDto.Name))
            {
                return BadRequest(ApiResponse.Fail("部门名称不能为空"));
            }

            var response = await _departmentService.UpdateDepartmentAsync(id, departmentDto.Name);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 删除部门
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var response = await _departmentService.DeleteDepartmentAsync(id);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }
    }
}
