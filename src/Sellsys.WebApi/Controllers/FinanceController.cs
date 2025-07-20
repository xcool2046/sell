using Microsoft.AspNetCore.Mvc;
using Sellsys.Application.DTOs.Finance;
using Sellsys.Application.Interfaces;

namespace Sellsys.WebApi.Controllers
{
    /// <summary>
    /// 财务管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FinanceController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        /// <summary>
        /// 获取财务订单明细列表
        /// </summary>
        /// <param name="filter">筛选条件</param>
        /// <returns>财务订单明细列表</returns>
        [HttpPost("order-details")]
        public async Task<IActionResult> GetFinanceOrderDetails([FromBody] FinanceFilterDto filter)
        {
            var response = await _financeService.GetFinanceOrderDetailsAsync(filter);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 获取财务订单明细列表（GET方式，用于简单查询）
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="productId">产品ID</param>
        /// <param name="salesPersonId">销售人员ID</param>
        /// <param name="orderStatus">订单状态</param>
        /// <param name="searchKeyword">搜索关键词</param>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>财务订单明细列表</returns>
        [HttpGet("order-details")]
        public async Task<IActionResult> GetFinanceOrderDetails(
            [FromQuery] int? customerId = null,
            [FromQuery] int? productId = null,
            [FromQuery] int? salesPersonId = null,
            [FromQuery] string? orderStatus = null,
            [FromQuery] string? searchKeyword = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var filter = new FinanceFilterDto
            {
                CustomerId = customerId,
                ProductId = productId,
                SalesPersonId = salesPersonId,
                OrderStatus = orderStatus,
                SearchKeyword = searchKeyword,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var response = await _financeService.GetFinanceOrderDetailsAsync(filter);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 获取筛选数据源
        /// </summary>
        /// <returns>筛选数据源</returns>
        [HttpGet("filter-options")]
        public async Task<IActionResult> GetFilterOptions()
        {
            var response = await _financeService.GetFilterOptionsAsync();
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 更新收款信息
        /// </summary>
        /// <param name="updateDto">更新信息</param>
        /// <returns>操作结果</returns>
        [HttpPut("payment-info")]
        public async Task<IActionResult> UpdatePaymentInfo([FromBody] UpdatePaymentInfoDto updateDto)
        {
            var response = await _financeService.UpdatePaymentInfoAsync(updateDto);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 批量更新收款信息
        /// </summary>
        /// <param name="batchUpdateDto">批量更新信息</param>
        /// <returns>操作结果</returns>
        [HttpPut("payment-info/batch")]
        public async Task<IActionResult> BatchUpdatePaymentInfo([FromBody] BatchUpdatePaymentInfoDto batchUpdateDto)
        {
            var response = await _financeService.BatchUpdatePaymentInfoAsync(batchUpdateDto);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 获取财务汇总信息
        /// </summary>
        /// <param name="filter">筛选条件</param>
        /// <returns>汇总信息</returns>
        [HttpPost("summary")]
        public async Task<IActionResult> GetFinanceSummary([FromBody] FinanceFilterDto filter)
        {
            var response = await _financeService.GetFinanceSummaryAsync(filter);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 获取财务汇总信息（GET方式）
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="productId">产品ID</param>
        /// <param name="salesPersonId">销售人员ID</param>
        /// <param name="orderStatus">订单状态</param>
        /// <returns>汇总信息</returns>
        [HttpGet("summary")]
        public async Task<IActionResult> GetFinanceSummary(
            [FromQuery] int? customerId = null,
            [FromQuery] int? productId = null,
            [FromQuery] int? salesPersonId = null,
            [FromQuery] string? orderStatus = null)
        {
            var filter = new FinanceFilterDto
            {
                CustomerId = customerId,
                ProductId = productId,
                SalesPersonId = salesPersonId,
                OrderStatus = orderStatus
            };

            var response = await _financeService.GetFinanceSummaryAsync(filter);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 导出财务数据
        /// </summary>
        /// <param name="exportDto">导出参数</param>
        /// <returns>导出文件</returns>
        [HttpPost("export")]
        public async Task<IActionResult> ExportFinanceData([FromBody] ExportFinanceDataDto exportDto)
        {
            var response = await _financeService.ExportFinanceDataAsync(exportDto);
            
            if (!response.IsSuccess || response.Data == null)
            {
                return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
            }

            var fileName = $"财务数据_{DateTime.Now:yyyyMMdd_HHmmss}.{exportDto.ExportFormat.ToLower()}";
            var contentType = exportDto.ExportFormat.ToLower() switch
            {
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "csv" => "text/csv",
                _ => "application/octet-stream"
            };

            return File(response.Data, contentType, fileName);
        }

        /// <summary>
        /// 获取订单的收款历史记录
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>收款历史记录</returns>
        [HttpGet("payment-history/{orderId}")]
        public async Task<IActionResult> GetPaymentHistory(int orderId)
        {
            var response = await _financeService.GetPaymentHistoryAsync(orderId);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        /// <summary>
        /// 确认收款（兼容原有API）
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="paymentDate">收款日期</param>
        /// <returns>操作结果</returns>
        [HttpPost("orders/{orderId}/confirm-payment")]
        public async Task<IActionResult> ConfirmPayment(int orderId, [FromBody] DateTime? paymentDate = null)
        {
            var updateDto = new UpdatePaymentInfoDto
            {
                OrderId = orderId,
                PaymentReceivedDate = paymentDate ?? DateTime.Now,
                ReceivedAmount = 0 // 这里需要根据实际业务逻辑来设置
            };

            var response = await _financeService.UpdatePaymentInfoAsync(updateDto);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }
    }
}
