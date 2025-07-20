namespace Sellsys.WpfClient.Constants
{
    /// <summary>
    /// 系统模块常量定义
    /// </summary>
    public static class SystemModules
    {
        /// <summary>
        /// 所有系统模块列表
        /// </summary>
        public static readonly List<string> AllModules = new List<string>
        {
            CustomerManagement,
            ProductManagement,
            OrderManagement,
            SalesFollowUp,
            AfterSalesService,
            FinanceManagement,
            SystemSettings
        };

        /// <summary>
        /// 客户管理模块
        /// </summary>
        public const string CustomerManagement = "客户管理";

        /// <summary>
        /// 产品管理模块
        /// </summary>
        public const string ProductManagement = "产品管理";

        /// <summary>
        /// 订单管理模块
        /// </summary>
        public const string OrderManagement = "订单管理";

        /// <summary>
        /// 销售跟进模块
        /// </summary>
        public const string SalesFollowUp = "销售跟进";

        /// <summary>
        /// 售后服务模块
        /// </summary>
        public const string AfterSalesService = "售后服务";

        /// <summary>
        /// 财务管理模块
        /// </summary>
        public const string FinanceManagement = "财务管理";

        /// <summary>
        /// 系统设置模块
        /// </summary>
        public const string SystemSettings = "系统设置";

        /// <summary>
        /// 获取模块的显示名称
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <returns>显示名称</returns>
        public static string GetDisplayName(string moduleName)
        {
            return moduleName;
        }

        /// <summary>
        /// 检查模块是否有效
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <returns>是否有效</returns>
        public static bool IsValidModule(string moduleName)
        {
            return AllModules.Contains(moduleName);
        }
    }
}
