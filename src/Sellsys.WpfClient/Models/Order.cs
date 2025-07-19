using System.Collections.ObjectModel;

namespace Sellsys.WpfClient.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }
        public DateTime? PaymentReceivedDate { get; set; }
        public decimal? SalesCommissionAmount { get; set; }
        public decimal? SupervisorCommissionAmount { get; set; }
        public decimal? ManagerCommissionAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ObservableCollection<OrderItem> OrderItems { get; set; } = new ObservableCollection<OrderItem>();

        // 计算属性
        public decimal TotalAmount => OrderItems.Sum(item => item.TotalAmount);
        public int TotalQuantity => OrderItems.Sum(item => item.Quantity);
        public string FormattedTotalAmount => TotalAmount.ToString("C");
        public string FormattedCreatedAt => CreatedAt.ToString("yyyy-MM-dd HH:mm");
        public string FormattedUpdatedAt => UpdatedAt.ToString("yyyy-MM-dd HH:mm");
        public string FormattedEffectiveDate => EffectiveDate?.ToString("yyyy-MM-dd") ?? "未设置";
        public string FormattedExpiryDate => ExpiryDate?.ToString("yyyy-MM-dd") ?? "未设置";
        public string FormattedPaymentReceivedDate => PaymentReceivedDate?.ToString("yyyy-MM-dd") ?? "未收款";
        
        // 状态显示
        public string StatusDisplay => Status switch
        {
            "Draft" => "草稿",
            "Confirmed" => "已确认",
            "Shipped" => "已发货",
            "Delivered" => "已交付",
            "Cancelled" => "已取消",
            _ => Status
        };

        // 是否可以编辑
        public bool CanEdit => Status == "Draft";
        
        // 是否可以取消
        public bool CanCancel => Status is "Draft" or "Confirmed";
    }
}
