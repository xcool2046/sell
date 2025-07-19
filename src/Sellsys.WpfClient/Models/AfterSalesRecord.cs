using System.ComponentModel;

namespace Sellsys.WpfClient.Models
{
    public class AfterSalesRecord : INotifyPropertyChanged
    {
        private bool _isSelected;

        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerProvince { get; set; }
        public string? CustomerCity { get; set; }
        public int? ContactId { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? CustomerFeedback { get; set; }
        public string? OurReply { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ProcessedDate { get; set; }
        public int? SupportPersonId { get; set; }
        public string? SupportPersonName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 显示用的格式化属性
        public string FormattedCreatedAt => CreatedAt.ToString("yyyy-MM-dd HH:mm");
        public string FormattedUpdatedAt => UpdatedAt.ToString("yyyy-MM-dd HH:mm");
        public string FormattedProcessedDate => ProcessedDate?.ToString("yyyy-MM-dd HH:mm") ?? "未处理";
        public string ContactInfo => !string.IsNullOrEmpty(ContactName) ? ContactName : "无指定联系人";
        public string FeedbackSummary => !string.IsNullOrEmpty(CustomerFeedback) 
            ? (CustomerFeedback.Length > 50 ? CustomerFeedback.Substring(0, 50) + "..." : CustomerFeedback)
            : "无反馈内容";
        public string ReplySummary => !string.IsNullOrEmpty(OurReply) 
            ? (OurReply.Length > 50 ? OurReply.Substring(0, 50) + "..." : OurReply)
            : "未回复";

        // 状态显示
        public string StatusDisplay => Status switch
        {
            "Pending" => "待处理",
            "InProgress" => "处理中",
            "Resolved" => "已解决",
            "Closed" => "已关闭",
            _ => Status
        };

        // 状态颜色（用于UI显示）
        public string StatusColor => Status switch
        {
            "Pending" => "#E6A23C",      // 橙色
            "InProgress" => "#409EFF",   // 蓝色
            "Resolved" => "#67C23A",     // 绿色
            "Closed" => "#909399",       // 灰色
            _ => "#303133"               // 默认黑色
        };

        // 是否可以编辑
        public bool CanEdit => Status is "Pending" or "InProgress";
        
        // 是否可以关闭
        public bool CanClose => Status is "Resolved";

        // 优先级（根据创建时间和状态计算）
        public string Priority
        {
            get
            {
                if (Status == "Pending")
                {
                    var daysSinceCreated = (DateTime.Now - CreatedAt).Days;
                    return daysSinceCreated switch
                    {
                        > 7 => "高",
                        > 3 => "中",
                        _ => "低"
                    };
                }
                return "普通";
            }
        }

        public string PriorityColor => Priority switch
        {
            "高" => "#F56C6C",    // 红色
            "中" => "#E6A23C",    // 橙色
            "低" => "#67C23A",    // 绿色
            _ => "#909399"        // 灰色
        };
    }
}
