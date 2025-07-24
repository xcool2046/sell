using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Sellsys.WpfClient.Services
{
    /// <summary>
    /// 认证服务
    /// </summary>
    public class AuthenticationService
    {
        private readonly ApiService _apiService;

        // 系统模块常量 - 使用统一的中文名称
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

        public AuthenticationService(ApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>登录结果</returns>
        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    return new LoginResult { Success = false, Message = "用户名和密码不能为空" };
                }

                // 检查是否为管理员账号
                if (username.Equals("admin", StringComparison.OrdinalIgnoreCase) && password == "admin")
                {
                    var adminUser = new UserInfo
                    {
                        Id = 0,
                        Username = "admin",
                        DisplayName = "系统管理员",
                        IsAdmin = true,
                        RoleName = "系统管理员",
                        AccessibleModules = GetAllModules()
                    };

                    CurrentUser.SetUser(adminUser);
                    return new LoginResult { Success = true, Message = "登录成功", User = adminUser };
                }

                // 验证员工账号
                return await ValidateEmployeeAsync(username, password);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"登录异常: {ex.Message}");
                return new LoginResult { Success = false, Message = "登录过程中发生错误，请稍后重试" };
            }
        }

        /// <summary>
        /// 验证员工账号
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>验证结果</returns>
        private async Task<LoginResult> ValidateEmployeeAsync(string username, string password)
        {
            try
            {
                // 调用登录API进行验证
                var loginResponse = await _apiService.LoginAsync(username, password);
                if (loginResponse == null)
                {
                    return new LoginResult { Success = false, Message = "用户名或密码错误" };
                }

                // 解析可访问模块
                var accessibleModules = new List<string>();
                if (!string.IsNullOrEmpty(loginResponse.AccessibleModules))
                {
                    accessibleModules = loginResponse.AccessibleModules
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(m => m.Trim())
                        .ToList();
                }

                var userInfo = new UserInfo
                {
                    Id = loginResponse.Id,
                    Username = loginResponse.Username,
                    DisplayName = loginResponse.DisplayName,
                    IsAdmin = loginResponse.IsAdmin,
                    RoleId = loginResponse.RoleId,
                    RoleName = loginResponse.RoleName,
                    DepartmentName = loginResponse.DepartmentName,
                    GroupId = loginResponse.GroupId,
                    GroupName = loginResponse.GroupName,
                    AccessibleModules = accessibleModules
                };

                CurrentUser.SetUser(userInfo);
                return new LoginResult { Success = true, Message = "登录成功", User = userInfo };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"员工验证异常: {ex.Message}");
                return new LoginResult { Success = false, Message = ex.Message.Contains("用户名或密码错误") ? "用户名或密码错误" : "验证员工信息时发生错误" };
            }
        }

        /// <summary>
        /// 获取所有系统模块
        /// </summary>
        /// <returns>模块列表</returns>
        private static List<string> GetAllModules()
        {
            return new List<string>
            {
                SystemModules.CustomerManagement,
                SystemModules.ProductManagement,
                SystemModules.OrderManagement,
                SystemModules.SalesFollowUp,
                SystemModules.AfterSalesService,
                SystemModules.FinanceManagement,
                SystemModules.SystemSettings
            };
        }

        /// <summary>
        /// 登出
        /// </summary>
        public void Logout()
        {
            CurrentUser.ClearUser();
        }
    }

    /// <summary>
    /// 登录结果
    /// </summary>
    public class LoginResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 用户信息
        /// </summary>
        public UserInfo? User { get; set; }
    }
}
