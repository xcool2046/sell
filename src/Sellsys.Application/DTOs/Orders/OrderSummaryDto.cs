namespace Sellsys.Application.DTOs.Orders
{
    /// <summary>
    /// 订单统计信息DTO
    /// </summary>
    public class OrderSummaryDto
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
    }
}
