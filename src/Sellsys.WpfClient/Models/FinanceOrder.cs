namespace Sellsys.WpfClient.Models
{
    public class FinanceOrder
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime? PaymentReceivedDate { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public int SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }
        public decimal? SalesCommissionAmount { get; set; }
        public decimal? SupervisorCommissionAmount { get; set; }
        public decimal? ManagerCommissionAmount { get; set; }
        public bool CommissionPaid { get; set; }
        public DateTime? CommissionPaidDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // 显示用的格式化属性
        public string FormattedTotalAmount => TotalAmount.ToString("C");
        public string FormattedSalesCommission => SalesCommissionAmount?.ToString("C") ?? "未设置";
        public string FormattedSupervisorCommission => SupervisorCommissionAmount?.ToString("C") ?? "未设置";
        public string FormattedManagerCommission => ManagerCommissionAmount?.ToString("C") ?? "未设置";
        public string FormattedPaymentReceivedDate => PaymentReceivedDate?.ToString("yyyy-MM-dd") ?? "未收款";
        public string FormattedCommissionPaidDate => CommissionPaidDate?.ToString("yyyy-MM-dd") ?? "未支付";
        public string FormattedCreatedAt => CreatedAt.ToString("yyyy-MM-dd HH:mm");

        // 支付状态显示
        public string PaymentStatusDisplay => PaymentStatus switch
        {
            "Pending" => "待收款",
            "Partial" => "部分收款",
            "Paid" => "已收款",
            "Overdue" => "逾期",
            _ => PaymentStatus
        };

        // 提成状态显示
        public string CommissionStatusDisplay => CommissionPaid ? "已支付" : "未支付";

        // 状态颜色（用于UI显示）
        public string PaymentStatusColor => PaymentStatus switch
        {
            "Pending" => "#E6A23C",      // 橙色
            "Partial" => "#409EFF",      // 蓝色
            "Paid" => "#67C23A",         // 绿色
            "Overdue" => "#F56C6C",      // 红色
            _ => "#303133"               // 默认黑色
        };

        public string CommissionStatusColor => CommissionPaid ? "#67C23A" : "#E6A23C";

        // 总提成金额
        public decimal TotalCommissionAmount => 
            (SalesCommissionAmount ?? 0) + 
            (SupervisorCommissionAmount ?? 0) + 
            (ManagerCommissionAmount ?? 0);

        public string FormattedTotalCommission => TotalCommissionAmount.ToString("C");

        // 是否可以确认收款
        public bool CanConfirmPayment => PaymentStatus != "Paid";

        // 是否可以支付提成
        public bool CanPayCommission => PaymentStatus == "Paid" && !CommissionPaid;

        // 收款进度百分比（用于进度条显示）
        public double PaymentProgress => PaymentStatus switch
        {
            "Pending" => 0,
            "Partial" => 50,
            "Paid" => 100,
            "Overdue" => 0,
            _ => 0
        };
    }
}
