using System.ComponentModel.DataAnnotations;

namespace Sellsys.Application.DTOs.Finance
{
    /// <summary>
    /// 更新收款信息DTO
    /// </summary>
    public class UpdatePaymentInfoDto
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        [Required]
        public int OrderId { get; set; }

        /// <summary>
        /// 已收金额
        /// </summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "已收金额不能为负数")]
        public decimal ReceivedAmount { get; set; }

        /// <summary>
        /// 收款日期
        /// </summary>
        public DateTime? PaymentReceivedDate { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500, ErrorMessage = "备注不能超过500个字符")]
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// 批量更新收款信息DTO
    /// </summary>
    public class BatchUpdatePaymentInfoDto
    {
        /// <summary>
        /// 更新项列表
        /// </summary>
        [Required]
        public List<UpdatePaymentInfoDto> Items { get; set; } = new List<UpdatePaymentInfoDto>();
    }

    /// <summary>
    /// 财务操作结果DTO
    /// </summary>
    public class FinanceOperationResultDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 受影响的订单数量
        /// </summary>
        public int AffectedOrderCount { get; set; }

        /// <summary>
        /// 错误详情
        /// </summary>
        public List<string> ErrorDetails { get; set; } = new List<string>();
    }

    /// <summary>
    /// 导出财务数据DTO
    /// </summary>
    public class ExportFinanceDataDto
    {
        /// <summary>
        /// 筛选条件
        /// </summary>
        public FinanceFilterDto Filter { get; set; } = new FinanceFilterDto();

        /// <summary>
        /// 导出格式
        /// </summary>
        public string ExportFormat { get; set; } = "Excel"; // Excel, CSV

        /// <summary>
        /// 是否包含汇总信息
        /// </summary>
        public bool IncludeSummary { get; set; } = true;

        /// <summary>
        /// 导出的列
        /// </summary>
        public List<string> ExportColumns { get; set; } = new List<string>();
    }
}
