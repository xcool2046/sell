using System.Collections.Generic;

namespace Sellsys.WpfClient.Models
{
    /// <summary>
    /// 订单统计信息模型
    /// </summary>
    public class OrderSummary
    {
        /// <summary>
        /// 订单总数
        /// </summary>
        public int TotalOrders { get; set; }

        /// <summary>
        /// 订单总金额
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 产品总数量
        /// </summary>
        public int TotalQuantity { get; set; }

        /// <summary>
        /// 平均订单金额
        /// </summary>
        public decimal AverageOrderAmount { get; set; }

        /// <summary>
        /// 各状态订单数量统计
        /// </summary>
        public Dictionary<string, int> StatusCounts { get; set; } = new Dictionary<string, int>();

        // 格式化属性用于界面显示
        public string FormattedTotalAmount => TotalAmount.ToString("C");
        public string FormattedAverageOrderAmount => AverageOrderAmount.ToString("C");
    }
}
