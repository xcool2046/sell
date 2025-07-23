using Microsoft.EntityFrameworkCore;
using Sellsys.Domain.Entities;
using Sellsys.Infrastructure.Data;

namespace Sellsys.Application.Services
{
    /// <summary>
    /// 角色种子数据服务
    /// 负责在应用启动时创建默认的岗位职务角色
    /// </summary>
    public class RoleSeedService
    {
        private readonly SellsysDbContext _context;

        public RoleSeedService(SellsysDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 初始化默认角色数据
        /// </summary>
        public async Task SeedRolesAsync()
        {
            try
            {
                // 定义默认角色及其权限配置
                var defaultRoles = GetDefaultRoles();

                foreach (var roleConfig in defaultRoles)
                {
                    // 检查角色是否已存在
                    var existingRole = await _context.Roles
                        .FirstOrDefaultAsync(r => r.Name == roleConfig.Name);

                    if (existingRole == null)
                    {
                        // 创建新角色
                        var newRole = new Role
                        {
                            Name = roleConfig.Name,
                            AccessibleModules = string.Join(",", roleConfig.AccessibleModules),
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Roles.Add(newRole);
                        Console.WriteLine($"创建角色: {roleConfig.Name}");
                    }
                    else
                    {
                        Console.WriteLine($"角色已存在: {roleConfig.Name}");
                    }
                }

                // 保存更改
                await _context.SaveChangesAsync();
                Console.WriteLine("角色种子数据初始化完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"角色种子数据初始化失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取默认角色配置
        /// </summary>
        private List<RoleConfiguration> GetDefaultRoles()
        {
            // 系统模块常量
            const string CustomerManagement = "客户管理";
            const string ProductManagement = "产品管理";
            const string OrderManagement = "订单管理";
            const string SalesFollowUp = "销售跟进";
            const string AfterSalesService = "售后服务";
            const string FinanceManagement = "财务管理";
            const string SystemSettings = "系统设置";

            return new List<RoleConfiguration>
            {
                // 销售经理 - 全部权限（管理级别）
                new RoleConfiguration
                {
                    Name = "销售经理",
                    AccessibleModules = new List<string>
                    {
                        CustomerManagement,
                        ProductManagement,
                        OrderManagement,
                        SalesFollowUp,
                        AfterSalesService,
                        FinanceManagement,
                        SystemSettings
                    }
                },

                // 销售主管 - 销售相关权限 + 部分管理权限
                new RoleConfiguration
                {
                    Name = "销售主管",
                    AccessibleModules = new List<string>
                    {
                        CustomerManagement,
                        ProductManagement,
                        OrderManagement,
                        SalesFollowUp,
                        FinanceManagement
                    }
                },

                // 销售 - 基础销售权限
                new RoleConfiguration
                {
                    Name = "销售",
                    AccessibleModules = new List<string>
                    {
                        CustomerManagement,
                        ProductManagement,
                        OrderManagement,
                        SalesFollowUp
                    }
                },

                // 客服经理 - 客服相关权限 + 管理权限
                new RoleConfiguration
                {
                    Name = "客服经理",
                    AccessibleModules = new List<string>
                    {
                        CustomerManagement,
                        OrderManagement,
                        AfterSalesService,
                        FinanceManagement,
                        SystemSettings
                    }
                },

                // 客服主管 - 客服相关权限 + 部分管理权限
                new RoleConfiguration
                {
                    Name = "客服主管",
                    AccessibleModules = new List<string>
                    {
                        CustomerManagement,
                        OrderManagement,
                        AfterSalesService,
                        FinanceManagement
                    }
                },

                // 客服 - 基础客服权限
                new RoleConfiguration
                {
                    Name = "客服",
                    AccessibleModules = new List<string>
                    {
                        CustomerManagement,
                        OrderManagement,
                        AfterSalesService
                    }
                }
            };
        }

        /// <summary>
        /// 角色配置类
        /// </summary>
        private class RoleConfiguration
        {
            public string Name { get; set; } = string.Empty;
            public List<string> AccessibleModules { get; set; } = new List<string>();
        }
    }
}
