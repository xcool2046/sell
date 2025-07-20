using System.ComponentModel;

namespace Sellsys.WpfClient.Models
{
    /// <summary>
    /// 客户售后服务聚合模型
    /// 用于售后服务主界面显示客户信息和售后记录统计
    /// </summary>
    public class CustomerAfterSales : INotifyPropertyChanged
    {
        private bool _isSelected;

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
        /// 最新更新时间
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

        /// <summary>
        /// 选中状态
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        /// <summary>
        /// 显示用的完整地址
        /// </summary>
        public string FullAddress => $"{Province}{City}".Trim();
        
        /// <summary>
        /// 联系人显示文本（主要联系人姓名或联系人数量）
        /// </summary>
        public string ContactDisplay => !string.IsNullOrEmpty(ContactName) ? ContactName : $"{ContactCount}人";
        
        /// <summary>
        /// 销售人员显示文本
        /// </summary>
        public string SalesPersonDisplay => SalesPersonName ?? "未分配";
        
        /// <summary>
        /// 客服人员显示文本
        /// </summary>
        public string SupportPersonDisplay => SupportPersonName ?? "未分配";
        
        /// <summary>
        /// 状态显示文本
        /// </summary>
        public string StatusDisplay
        {
            get
            {
                if (ServiceRecordCount == 0) return "无记录";
                if (PendingRecordCount > 0) return $"待处理({PendingRecordCount})";
                if (ProcessingRecordCount > 0) return $"处理中({ProcessingRecordCount})";
                return "已完成";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
