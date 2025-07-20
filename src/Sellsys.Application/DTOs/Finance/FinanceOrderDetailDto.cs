namespace Sellsys.Application.DTOs.Finance
{
    /// <summary>
    /// 财务订单明细DTO - 对应原型图的表格数据
    /// </summary>
    public class FinanceOrderDetailDto
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>
        /// 订单项ID
        /// </summary>
        public int OrderItemId { get; set; }

        /// <summary>
        /// 序号（前端生成）
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// 客户ID
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// 产品ID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// 产品规格
        /// </summary>
        public string? ProductSpecification { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// 总金额（数量 × 单价）
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 已收金额（按比例分摊到订单项）
        /// </summary>
        public decimal ReceivedAmount { get; set; }

        /// <summary>
        /// 未收金额（总金额 - 已收金额）
        /// </summary>
        public decimal UnreceivedAmount { get; set; }

        /// <summary>
        /// 收款比例（已收金额 / 总金额 × 100%）
        /// </summary>
        public decimal PaymentRatio { get; set; }

        /// <summary>
        /// 收款日期
        /// </summary>
        public DateTime? PaymentReceivedDate { get; set; }

        /// <summary>
        /// 负责人ID
        /// </summary>
        public int SalesPersonId { get; set; }

        /// <summary>
        /// 负责人姓名
        /// </summary>
        public string SalesPersonName { get; set; } = string.Empty;

        /// <summary>
        /// 生效日期
        /// </summary>
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// 到期日期
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public string OrderStatus { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        // 格式化显示属性
        /// <summary>
        /// 格式化的单价
        /// </summary>
        public string FormattedUnitPrice => UnitPrice.ToString("C");

        /// <summary>
        /// 格式化的总金额
        /// </summary>
        public string FormattedTotalAmount => TotalAmount.ToString("C");

        /// <summary>
        /// 格式化的已收金额
        /// </summary>
        public string FormattedReceivedAmount => ReceivedAmount.ToString("C");

        /// <summary>
        /// 格式化的未收金额
        /// </summary>
        public string FormattedUnreceivedAmount => UnreceivedAmount.ToString("C");

        /// <summary>
        /// 格式化的收款比例
        /// </summary>
        public string FormattedPaymentRatio => $"{PaymentRatio:F1}%";

        /// <summary>
        /// 格式化的收款日期
        /// </summary>
        public string FormattedPaymentReceivedDate => PaymentReceivedDate?.ToString("yyyy-MM-dd") ?? "未收款";

        /// <summary>
        /// 格式化的生效日期
        /// </summary>
        public string FormattedEffectiveDate => EffectiveDate?.ToString("yyyy-MM-dd") ?? "未设置";

        /// <summary>
        /// 格式化的到期日期
        /// </summary>
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
    }

    /// <summary>
    /// 财务订单明细列表响应DTO
    /// </summary>
    public class FinanceOrderDetailListDto
    {
        /// <summary>
        /// 订单明细列表
        /// </summary>
        public List<FinanceOrderDetailDto> Items { get; set; } = new List<FinanceOrderDetailDto>();

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// 合计信息
        /// </summary>
        public FinanceOrderSummaryDto Summary { get; set; } = new FinanceOrderSummaryDto();
    }

    /// <summary>
    /// 财务订单汇总DTO
    /// </summary>
    public class FinanceOrderSummaryDto
    {
        /// <summary>
        /// 总金额合计
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 已收金额合计
        /// </summary>
        public decimal TotalReceivedAmount { get; set; }

        /// <summary>
        /// 未收金额合计
        /// </summary>
        public decimal TotalUnreceivedAmount { get; set; }

        /// <summary>
        /// 收款比例合计
        /// </summary>
        public decimal TotalPaymentRatio { get; set; }

        /// <summary>
        /// 订单数量
        /// </summary>
        public int OrderCount { get; set; }

        /// <summary>
        /// 订单项数量
        /// </summary>
        public int OrderItemCount { get; set; }

        // 格式化显示属性
        public string FormattedTotalAmount => TotalAmount.ToString("C");
        public string FormattedTotalReceivedAmount => TotalReceivedAmount.ToString("C");
        public string FormattedTotalUnreceivedAmount => TotalUnreceivedAmount.ToString("C");
        public string FormattedTotalPaymentRatio => $"{TotalPaymentRatio:F1}%";
    }
}
