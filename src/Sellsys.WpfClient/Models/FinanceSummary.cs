namespace Sellsys.WpfClient.Models
{
    public class FinanceSummary
    {
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalReceivedAmount { get; set; }
        public decimal TotalPendingAmount { get; set; }
        public decimal TotalCommissionAmount { get; set; }
        public decimal TotalPaidCommissionAmount { get; set; }
        public decimal TotalUnpaidCommissionAmount { get; set; }
        public int TotalOrderCount { get; set; }
        public int PaidOrderCount { get; set; }
        public int PendingOrderCount { get; set; }
        public int OverdueOrderCount { get; set; }

        // 格式化显示
        public string FormattedTotalOrderAmount => TotalOrderAmount.ToString("C");
        public string FormattedTotalReceivedAmount => TotalReceivedAmount.ToString("C");
        public string FormattedTotalPendingAmount => TotalPendingAmount.ToString("C");
        public string FormattedTotalCommissionAmount => TotalCommissionAmount.ToString("C");
        public string FormattedTotalPaidCommissionAmount => TotalPaidCommissionAmount.ToString("C");
        public string FormattedTotalUnpaidCommissionAmount => TotalUnpaidCommissionAmount.ToString("C");

        // 百分比计算
        public double PaymentRate => TotalOrderAmount > 0 ? (double)(TotalReceivedAmount / TotalOrderAmount) * 100 : 0;
        public double CommissionPaymentRate => TotalCommissionAmount > 0 ? (double)(TotalPaidCommissionAmount / TotalCommissionAmount) * 100 : 0;

        public string FormattedPaymentRate => $"{PaymentRate:F1}%";
        public string FormattedCommissionPaymentRate => $"{CommissionPaymentRate:F1}%";
    }

    public class MonthlySalesData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public int OrderCount { get; set; }

        public string MonthName => $"{Year}年{Month}月";
        public string FormattedTotalAmount => TotalAmount.ToString("C");
        public string FormattedReceivedAmount => ReceivedAmount.ToString("C");
        public double CollectionRate => TotalAmount > 0 ? (double)(ReceivedAmount / TotalAmount) * 100 : 0;
        public string FormattedCollectionRate => $"{CollectionRate:F1}%";
    }

    public class SalesPersonCommission
    {
        public int SalesPersonId { get; set; }
        public string SalesPersonName { get; set; } = string.Empty;
        public decimal TotalSalesAmount { get; set; }
        public decimal TotalCommissionAmount { get; set; }
        public decimal PaidCommissionAmount { get; set; }
        public decimal UnpaidCommissionAmount { get; set; }
        public int OrderCount { get; set; }

        public string FormattedTotalSalesAmount => TotalSalesAmount.ToString("C");
        public string FormattedTotalCommissionAmount => TotalCommissionAmount.ToString("C");
        public string FormattedPaidCommissionAmount => PaidCommissionAmount.ToString("C");
        public string FormattedUnpaidCommissionAmount => UnpaidCommissionAmount.ToString("C");
        
        public double CommissionRate => TotalSalesAmount > 0 ? (double)(TotalCommissionAmount / TotalSalesAmount) * 100 : 0;
        public string FormattedCommissionRate => $"{CommissionRate:F2}%";
    }
}
