using Sellsys.Application.DTOs.Roles;
using Sellsys.Application.Interfaces;
using Sellsys.CrossCutting.Common;
using Sellsys.Domain.Entities;
using Sellsys.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;

namespace Sellsys.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly SellsysDbContext _context;

        public RoleService(SellsysDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<RoleDto>> CreateRoleAsync(RoleUpsertDto roleDto)
        {
            var role = new Sellsys.Domain.Entities.Role
            {
                Name = roleDto.Name,
                Department = roleDto.Department
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var resultDto = new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Department = role.Department,
                CreatedAt = role.CreatedAt
            };

            return ApiResponse<RoleDto>.Success(resultDto);
        }

        public async Task<ApiResponse> DeleteRoleAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return ApiResponse.Fail("Role not found.", HttpStatusCode.NotFound);
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return ApiResponse.Success();
        }

        public async Task<ApiResponse<List<RoleDto>>> GetAllRolesAsync()
        {
            var roles = await _context.Roles
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Department = r.Department,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<List<RoleDto>>.Success(roles);
        }

        public async Task<ApiResponse<RoleDto>> GetRoleByIdAsync(int id)
        {
            var role = await _context.Roles
                .Where(r => r.Id == id)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Department = r.Department,
                    CreatedAt = r.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return new ApiResponse<RoleDto> { IsSuccess = false, Message = "Role not found.", StatusCode = HttpStatusCode.NotFound };
            }

            return ApiResponse<RoleDto>.Success(role);
        }

        public async Task<ApiResponse> UpdateRoleAsync(int id, RoleUpsertDto roleDto)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return ApiResponse.Fail("Role not found.", HttpStatusCode.NotFound);
            }

            role.Name = roleDto.Name;
            role.Department = roleDto.Department;

            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            return ApiResponse.Success();
        }
    }
}