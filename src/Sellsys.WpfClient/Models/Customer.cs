using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Sellsys.WpfClient.Models
{
    public class Customer : INotifyPropertyChanged
    {
        private bool _isSelected;
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? Remarks { get; set; }
        public string? IndustryTypes { get; set; }
        public int? SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }
        public int? SupportPersonId { get; set; }
        public string? SupportPersonName { get; set; }
        public string? Status { get; set; } = "待跟进"; // 客户状态
        public DateTime CreatedAt { get; set; }

        public ObservableCollection<Contact> Contacts { get; set; } = new ObservableCollection<Contact>();

        // 显示用的完整地址
        public string FullAddress => $"{Province}{City}{Address}".Trim();
        
        // 主要联系人
        public Contact? PrimaryContact => Contacts.FirstOrDefault(c => c.IsPrimary);
        
        // 主要联系人姓名
        public string PrimaryContactName => PrimaryContact?.Name ?? "无";
        
        // 主要联系人电话
        public string PrimaryContactPhone => PrimaryContact?.Phone ?? "无";

        // 联系人数量
        public int ContactCount => Contacts?.Count ?? 0;

        // 客户意向
        public string CustomerIntention { get; set; } = "待分配";

        // 待办事项
        public string PendingTasks { get; set; } = "无";

        // 下次联系日期
        public DateTime? NextContactDate { get; set; }

        // 负责人姓名（优先显示销售，其次客服）
        public string ResponsiblePersonName => SalesPersonName ?? SupportPersonName ?? "未分配";

        // 选中状态
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
