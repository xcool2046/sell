using Microsoft.EntityFrameworkCore;
using Sellsys.Application.DTOs.Auth;
using Sellsys.Application.Interfaces;
using Sellsys.CrossCutting.Common;
using Sellsys.Infrastructure.Data;
using System.Net;

namespace Sellsys.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly SellsysDbContext _context;

        // 系统模块常量
        private static class SystemModules
        {
            public const string CustomerManagement = "客户管理";
            public const string ProductManagement = "产品管理";
            public const string OrderManagement = "订单管理";
            public const string SalesFollowUp = "销售跟进";
            public const string AfterSalesService = "售后服务";
            public const string FinanceManagement = "财务管理";
            public const string SystemSettings = "系统设置";
        }

        public AuthService(SellsysDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    return ApiResponse<LoginResponseDto>.Fail("用户名和密码不能为空", HttpStatusCode.BadRequest);
                }

                // 检查是否为管理员账号
                if (username.Equals("admin", StringComparison.OrdinalIgnoreCase) && password == "admin")
                {
                    var adminResponse = new LoginResponseDto
                    {
                        Id = 0,
                        Username = "admin",
                        DisplayName = "系统管理员",
                        IsAdmin = true,
                        RoleName = "系统管理员",
                        AccessibleModules = GetAllModulesString()
                    };

                    return ApiResponse<LoginResponseDto>.Success(adminResponse);
                }

                // 验证员工账号
                var employee = await _context.Employees
                    .Include(e => e.Group)
                        .ThenInclude(g => g!.Department)
                    .Include(e => e.Role)
                    .FirstOrDefaultAsync(e => e.LoginUsername == username);

                if (employee == null)
                {
                    return ApiResponse<LoginResponseDto>.Fail("用户名或密码错误", HttpStatusCode.Unauthorized);
                }

                // 验证密码
                if (string.IsNullOrEmpty(employee.HashedPassword) ||
                    !BCrypt.Net.BCrypt.Verify(password, employee.HashedPassword))
                {
                    return ApiResponse<LoginResponseDto>.Fail("用户名或密码错误", HttpStatusCode.Unauthorized);
                }

                // 构建响应
                var response = new LoginResponseDto
                {
                    Id = employee.Id,
                    Username = employee.LoginUsername,
                    DisplayName = employee.Name,
                    IsAdmin = false,
                    RoleId = employee.RoleId,
                    RoleName = employee.Role?.Name,
                    DepartmentName = employee.Group?.Department?.Name,
                    GroupId = employee.GroupId,
                    GroupName = employee.Group?.Name,
                    AccessibleModules = employee.Role?.AccessibleModules ?? string.Empty
                };

                return ApiResponse<LoginResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponseDto>.Fail($"登录过程中发生错误: {ex.Message}", HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// 获取所有系统模块字符串
        /// </summary>
        /// <returns>模块字符串</returns>
        private static string GetAllModulesString()
        {
            var modules = new[]
            {
                SystemModules.CustomerManagement,
                SystemModules.ProductManagement,
                SystemModules.OrderManagement,
                SystemModules.SalesFollowUp,
                SystemModules.AfterSalesService,
                SystemModules.FinanceManagement,
                SystemModules.SystemSettings
            };

            return string.Join(",", modules);
        }
    }
}
