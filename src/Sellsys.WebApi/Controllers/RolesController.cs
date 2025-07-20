using Microsoft.AspNetCore.Mvc;
using Sellsys.Application.DTOs.Roles;
using Sellsys.Application.Interfaces;
using System.Threading.Tasks;

namespace Sellsys.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var response = await _roleService.GetAllRolesAsync();
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            var response = await _roleService.GetRoleByIdAsync(id);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleUpsertDto roleDto)
        {
            var response = await _roleService.CreateRoleAsync(roleDto);
            if (response.IsSuccess && response.Data != null)
            {
                return CreatedAtAction(nameof(GetRoleById), new { id = response.Data.Id }, response);
            }
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleUpsertDto roleDto)
        {
            var response = await _roleService.UpdateRoleAsync(id, roleDto);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var response = await _roleService.DeleteRoleAsync(id);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }
    }
}