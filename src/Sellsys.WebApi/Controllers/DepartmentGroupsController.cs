using Microsoft.AspNetCore.Mvc;
using Sellsys.Application.Services;
using Sellsys.CrossCutting.Common;

namespace Sellsys.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentGroupsController : ControllerBase
    {
        private readonly DepartmentGroupService _departmentGroupService;

        public DepartmentGroupsController(DepartmentGroupService departmentGroupService)
        {
            _departmentGroupService = departmentGroupService;
        }

        /// <summary>
        /// 获取所有分组
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllGroups()
        {
            var response = await _departmentGroupService.GetAllGroupsAsync();
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 根据部门ID获取分组
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        [HttpGet("by-department/{departmentId}")]
        public async Task<IActionResult> GetGroupsByDepartmentId(int departmentId)
        {
            var response = await _departmentGroupService.GetGroupsByDepartmentIdAsync(departmentId);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 根据ID获取分组
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroupById(int id)
        {
            var response = await _departmentGroupService.GetGroupByIdAsync(id);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 创建新分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(ApiResponse.Fail("分组名称不能为空"));
            }

            if (request.DepartmentId <= 0)
            {
                return BadRequest(ApiResponse.Fail("部门ID无效"));
            }

            var response = await _departmentGroupService.CreateGroupAsync(request.DepartmentId, request.Name);
            if (response.IsSuccess && response.Data != null)
            {
                return CreatedAtAction(nameof(GetGroupById), new { id = response.Data.Id }, response);
            }
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 更新分组
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(int id, [FromBody] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(ApiResponse.Fail("分组名称不能为空"));
            }

            var response = await _departmentGroupService.UpdateGroupAsync(id, name);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 删除分组
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var response = await _departmentGroupService.DeleteGroupAsync(id);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }
    }

    public class CreateGroupRequest
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
