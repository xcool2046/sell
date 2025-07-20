using System.ComponentModel;

namespace Sellsys.WpfClient.Models
{
    /// <summary>
    /// 财务订单明细模型 - 对应原型图的表格数据
    /// </summary>
    public class FinanceOrderDetail : INotifyPropertyChanged
    {
        private bool _isSelected;

        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int OrderItemId { get; set; }
        public int RowNumber { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductSpecification { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal UnreceivedAmount { get; set; }
        public decimal PaymentRatio { get; set; }
        public DateTime? PaymentReceivedDate { get; set; }
        public int SalesPersonId { get; set; }
        public string SalesPersonName { get; set; } = string.Empty;
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 是否选中（用于批量操作）
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        // 格式化显示属性
        public string FormattedUnitPrice => UnitPrice.ToString("C");
        public string FormattedTotalAmount => TotalAmount.ToString("C");
        public string FormattedReceivedAmount => ReceivedAmount.ToString("C");
        public string FormattedUnreceivedAmount => UnreceivedAmount.ToString("C");
        public string FormattedPaymentRatio => $"{PaymentRatio:F1}%";
        public string FormattedPaymentReceivedDate => PaymentReceivedDate?.ToString("yyyy-MM-dd") ?? "未收款";
        public string FormattedEffectiveDate => EffectiveDate?.ToString("yyyy-MM-dd") ?? "未设置";
        public string FormattedExpiryDate => ExpiryDate?.ToString("yyyy-MM-dd") ?? "未设置";

        /// <summary>
        /// 产品完整信息（包含规格）
        /// </summary>
        public string ProductFullName => !string.IsNullOrEmpty(ProductSpecification) 
            ? $"{ProductName} ({ProductSpecification})" 
            : ProductName;

        /// <summary>
        /// 订单状态显示
        /// </summary>
        public string OrderStatusDisplay => OrderStatus switch
        {
            "待收款" => "待收款",
            "部分收款" => "部分收款",
            "已收款" => "已收款",
            "逾期" => "逾期",
            _ => OrderStatus
        };

        /// <summary>
        /// 收款状态颜色
        /// </summary>
        public string PaymentStatusColor => PaymentRatio switch
        {
            0 => "#E6A23C",           // 橙色 - 未收款
            >= 100 => "#67C23A",      // 绿色 - 已收款
            _ => "#409EFF"            // 蓝色 - 部分收款
        };

        /// <summary>
        /// 是否可以编辑收款信息
        /// </summary>
        public bool CanEditPayment => OrderStatus != "已收款";

        /// <summary>
        /// 是否逾期
        /// </summary>
        public bool IsOverdue => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Now && PaymentRatio < 100;

        /// <summary>
        /// 逾期天数
        /// </summary>
        public int OverdueDays => IsOverdue ? (DateTime.Now - ExpiryDate!.Value).Days : 0;

        /// <summary>
        /// 逾期显示文本
        /// </summary>
        public string OverdueDisplay => IsOverdue ? $"逾期{OverdueDays}天" : "";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 财务订单明细列表模型
    /// </summary>
    public class FinanceOrderDetailList
    {
        public List<FinanceOrderDetail> Items { get; set; } = new List<FinanceOrderDetail>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public FinanceOrderSummary Summary { get; set; } = new FinanceOrderSummary();

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// 财务订单汇总模型
    /// </summary>
    public class FinanceOrderSummary
    {
        public decimal TotalAmount { get; set; }
        public decimal TotalReceivedAmount { get; set; }
        public decimal TotalUnreceivedAmount { get; set; }
        public decimal TotalPaymentRatio { get; set; }
        public int OrderCount { get; set; }
        public int OrderItemCount { get; set; }

        // 格式化显示属性
        public string FormattedTotalAmount => TotalAmount.ToString("C");
        public string FormattedTotalReceivedAmount => TotalReceivedAmount.ToString("C");
        public string FormattedTotalUnreceivedAmount => TotalUnreceivedAmount.ToString("C");
        public string FormattedTotalPaymentRatio => $"{TotalPaymentRatio:F1}%";

        /// <summary>
        /// 收款进度百分比（用于进度条）
        /// </summary>
        public double PaymentProgressPercentage => TotalAmount == 0 ? 0 : (double)(TotalReceivedAmount / TotalAmount) * 100;
    }
}
