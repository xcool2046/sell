using Sellsys.Application.DTOs.Finance;
using Sellsys.CrossCutting.Common;

namespace Sellsys.Application.Interfaces
{
    /// <summary>
    /// 财务管理服务接口
    /// </summary>
    public interface IFinanceService
    {
        /// <summary>
        /// 获取财务订单明细列表
        /// </summary>
        /// <param name="filter">筛选条件</param>
        /// <returns>财务订单明细列表</returns>
        Task<ApiResponse<FinanceOrderDetailListDto>> GetFinanceOrderDetailsAsync(FinanceFilterDto filter);

        /// <summary>
        /// 获取筛选数据源
        /// </summary>
        /// <returns>筛选数据源</returns>
        Task<ApiResponse<FinanceFilterOptionsDto>> GetFilterOptionsAsync();

        /// <summary>
        /// 确认全额收款
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="paymentDate">收款日期</param>
        /// <returns>操作结果</returns>
        Task<ApiResponse<FinanceOperationResultDto>> ConfirmFullPaymentAsync(int orderId, DateTime paymentDate);

        /// <summary>
        /// 更新收款信息
        /// </summary>
        /// <param name="updateDto">更新信息</param>
        /// <returns>操作结果</returns>
        Task<ApiResponse<FinanceOperationResultDto>> UpdatePaymentInfoAsync(UpdatePaymentInfoDto updateDto);

        /// <summary>
        /// 批量更新收款信息
        /// </summary>
        /// <param name="batchUpdateDto">批量更新信息</param>
        /// <returns>操作结果</returns>
        Task<ApiResponse<FinanceOperationResultDto>> BatchUpdatePaymentInfoAsync(BatchUpdatePaymentInfoDto batchUpdateDto);

        /// <summary>
        /// 获取财务汇总信息
        /// </summary>
        /// <param name="filter">筛选条件</param>
        /// <returns>汇总信息</returns>
        Task<ApiResponse<FinanceOrderSummaryDto>> GetFinanceSummaryAsync(FinanceFilterDto filter);

        /// <summary>
        /// 导出财务数据
        /// </summary>
        /// <param name="exportDto">导出参数</param>
        /// <returns>导出文件数据</returns>
        Task<ApiResponse<byte[]>> ExportFinanceDataAsync(ExportFinanceDataDto exportDto);

        /// <summary>
        /// 获取订单的收款历史记录
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>收款历史记录</returns>
        Task<ApiResponse<List<PaymentHistoryDto>>> GetPaymentHistoryAsync(int orderId);
    }

    /// <summary>
    /// 收款历史记录DTO
    /// </summary>
    public class PaymentHistoryDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? Remarks { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string FormattedAmount => Amount.ToString("C");
        public string FormattedPaymentDate => PaymentDate.ToString("yyyy-MM-dd");
        public string FormattedCreatedAt => CreatedAt.ToString("yyyy-MM-dd HH:mm");
    }
}
