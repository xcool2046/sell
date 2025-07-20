namespace Sellsys.Application.DTOs.AfterSales
{
    /// <summary>
    /// 客户售后服务聚合数据传输对象
    /// 用于售后服务主界面显示客户信息和售后记录统计
    /// </summary>
    public class CustomerAfterSalesDto
    {
        /// <summary>
        /// 客户ID
        /// </summary>
        public int CustomerId { get; set; }
        
        /// <summary>
        /// 客户单位名称
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;
        
        /// <summary>
        /// 省份
        /// </summary>
        public string? Province { get; set; }
        
        /// <summary>
        /// 城市
        /// </summary>
        public string? City { get; set; }
        
        /// <summary>
        /// 主要联系人姓名
        /// </summary>
        public string? ContactName { get; set; }
        
        /// <summary>
        /// 主要联系人电话
        /// </summary>
        public string? ContactPhone { get; set; }
        
        /// <summary>
        /// 联系人总数
        /// </summary>
        public int ContactCount { get; set; }
        
        /// <summary>
        /// 销售人员ID
        /// </summary>
        public int? SalesPersonId { get; set; }
        
        /// <summary>
        /// 销售人员姓名
        /// </summary>
        public string? SalesPersonName { get; set; }
        
        /// <summary>
        /// 客服人员ID
        /// </summary>
        public int? SupportPersonId { get; set; }
        
        /// <summary>
        /// 客服人员姓名
        /// </summary>
        public string? SupportPersonName { get; set; }
        
        /// <summary>
        /// 客服记录总数
        /// </summary>
        public int ServiceRecordCount { get; set; }
        
        /// <summary>
        /// 最新更新时间（取客户更新时间和最新售后记录更新时间的较大值）
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// 客户创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// 最新售后记录状态
        /// </summary>
        public string? LatestRecordStatus { get; set; }
        
        /// <summary>
        /// 待处理记录数量
        /// </summary>
        public int PendingRecordCount { get; set; }
        
        /// <summary>
        /// 处理中记录数量
        /// </summary>
        public int ProcessingRecordCount { get; set; }
        
        /// <summary>
        /// 已完成记录数量
        /// </summary>
        public int CompletedRecordCount { get; set; }
    }
}
