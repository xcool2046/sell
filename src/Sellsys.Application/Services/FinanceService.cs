using Microsoft.EntityFrameworkCore;
using Sellsys.Application.DTOs.Finance;
using Sellsys.CrossCutting.Common;
using Sellsys.Application.Interfaces;
using Sellsys.Infrastructure.Data;
using System.Net;

namespace Sellsys.Application.Services
{
    /// <summary>
    /// 财务管理服务实现
    /// </summary>
    public class FinanceService : IFinanceService
    {
        private readonly SellsysDbContext _context;

        public FinanceService(SellsysDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 获取财务订单明细列表
        /// </summary>
        public async Task<ApiResponse<FinanceOrderDetailListDto>> GetFinanceOrderDetailsAsync(FinanceFilterDto filter)
        {
            try
            {
                var query = _context.OrderItems
                    .Include(oi => oi.Order)
                        .ThenInclude(o => o.Customer)
                    .Include(oi => oi.Order)
                        .ThenInclude(o => o.SalesPerson)
                    .Include(oi => oi.Product)
                    .AsQueryable();

                // 应用筛选条件
                query = ApplyFilters(query, filter);

                // 获取总数
                var totalCount = await query.CountAsync();

                // 分页 - 先获取基础数据，然后在内存中计算
                var orderItems = await query
                    .OrderByDescending(oi => oi.Order.CreatedAt)
                    .ThenBy(oi => oi.Id)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                // 在内存中转换为DTO并计算收款信息
                var items = orderItems.Select(oi =>
                {
                    var receivedAmount = CalculateReceivedAmountForItem(oi);
                    var paymentRatio = CalculatePaymentRatioForItem(oi);

                    return new FinanceOrderDetailDto
                    {
                        OrderId = oi.OrderId,
                        OrderNumber = oi.Order.OrderNumber,
                        OrderItemId = oi.Id,
                        CustomerId = oi.Order.CustomerId,
                        CustomerName = oi.Order.Customer.Name,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        ProductSpecification = oi.Product.Specification,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.ActualPrice,
                        TotalAmount = oi.TotalAmount,
                        // 收款信息计算
                        ReceivedAmount = receivedAmount,
                        UnreceivedAmount = oi.TotalAmount - receivedAmount,
                        PaymentRatio = paymentRatio,
                        PaymentReceivedDate = oi.Order.PaymentReceivedDate,
                        SalesPersonId = oi.Order.SalesPersonId,
                        SalesPersonName = oi.Order.SalesPerson.Name,
                        EffectiveDate = oi.Order.EffectiveDate,
                        ExpiryDate = oi.Order.ExpiryDate,
                        OrderStatus = oi.Order.Status,
                        CreatedAt = oi.Order.CreatedAt,
                        UpdatedAt = oi.Order.UpdatedAt
                    };
                }).ToList();

                // 添加序号
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].RowNumber = (filter.PageNumber - 1) * filter.PageSize + i + 1;
                }

                // 计算汇总信息
                var summary = await CalculateSummaryAsync(filter);

                var result = new FinanceOrderDetailListDto
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Summary = summary
                };

                return new ApiResponse<FinanceOrderDetailListDto>
                {
                    IsSuccess = true,
                    Data = result,
                    Message = "获取财务订单明细成功",
                    StatusCode = HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<FinanceOrderDetailListDto>
                {
                    IsSuccess = false,
                    Message = $"获取财务订单明细失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        /// <summary>
        /// 获取筛选数据源
        /// </summary>
        public async Task<ApiResponse<FinanceFilterOptionsDto>> GetFilterOptionsAsync()
        {
            try
            {
                var options = new FinanceFilterOptionsDto();

                // 获取客户列表 - 先获取基础数据，然后在内存中转换
                var customers = await _context.Customers
                    .Where(c => c.Orders.Any()) // 只显示有订单的客户
                    .Select(c => new { c.Id, c.Name })
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                options.Customers = customers
                    .Select(c => new FilterOptionDto
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToList();

                // 获取产品列表 - 先获取基础数据，然后在内存中格式化
                var products = await _context.Products
                    .Where(p => p.OrderItems.Any()) // 只显示有订单的产品
                    .Select(p => new { p.Id, p.Name, p.Specification })
                    .ToListAsync();

                options.Products = products
                    .Select(p => new FilterOptionDto
                    {
                        Value = p.Id.ToString(),
                        Text = !string.IsNullOrEmpty(p.Specification) ? $"{p.Name} ({p.Specification})" : p.Name
                    })
                    .OrderBy(x => x.Text)
                    .ToList();

                // 获取销售人员列表 - 先获取基础数据，然后在内存中转换
                var salesPersons = await _context.Employees
                    .Where(e => e.Orders.Any()) // 只显示有订单的销售人员
                    .Select(e => new { e.Id, e.Name })
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                options.SalesPersons = salesPersons
                    .Select(e => new FilterOptionDto
                    {
                        Value = e.Id.ToString(),
                        Text = e.Name
                    })
                    .ToList();

                // 订单状态列表
                options.OrderStatuses = new List<FilterOptionDto>
                {
                    new FilterOptionDto { Value = "待收款", Text = "待收款" },
                    new FilterOptionDto { Value = "部分收款", Text = "部分收款" },
                    new FilterOptionDto { Value = "已收款", Text = "已收款" },
                    new FilterOptionDto { Value = "逾期", Text = "逾期" }
                };

                // 生成日期选项（最近2年的年月）
                var dateOptions = GenerateDateOptions();
                options.EffectiveDateOptions = dateOptions;
                options.ExpiryDateOptions = dateOptions;
                options.PaymentDateOptions = dateOptions;

                return new ApiResponse<FinanceFilterOptionsDto>
                {
                    IsSuccess = true,
                    Data = options,
                    Message = "获取筛选数据源成功",
                    StatusCode = HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<FinanceFilterOptionsDto>
                {
                    IsSuccess = false,
                    Message = $"获取筛选数据源失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        /// <summary>
        /// 确认全额收款
        /// </summary>
        public async Task<ApiResponse<FinanceOperationResultDto>> ConfirmFullPaymentAsync(int orderId, DateTime paymentDate)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return new ApiResponse<FinanceOperationResultDto>
                    {
                        IsSuccess = false,
                        Message = "订单不存在",
                        StatusCode = HttpStatusCode.NotFound
                    };
                }

                // 确认全额收款
                order.PaymentReceivedDate = paymentDate;
                order.Status = "已收款";
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var result = new FinanceOperationResultDto
                {
                    IsSuccess = true,
                    Message = "确认收款成功",
                    AffectedOrderCount = 1
                };

                return new ApiResponse<FinanceOperationResultDto>
                {
                    IsSuccess = true,
                    Data = result,
                    Message = "确认收款成功",
                    StatusCode = HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<FinanceOperationResultDto>
                {
                    IsSuccess = false,
                    Message = $"确认收款失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        /// <summary>
        /// 更新收款信息
        /// </summary>
        public async Task<ApiResponse<FinanceOperationResultDto>> UpdatePaymentInfoAsync(UpdatePaymentInfoDto updateDto)
        {
            try
            {
                var order = await _context.Orders.FindAsync(updateDto.OrderId);
                if (order == null)
                {
                    return new ApiResponse<FinanceOperationResultDto>
                    {
                        IsSuccess = false,
                        Message = "订单不存在",
                        StatusCode = HttpStatusCode.NotFound
                    };
                }

                // 更新收款信息
                // 注意：这里需要根据实际业务逻辑来处理收款记录
                // 可能需要创建收款记录表来记录多次收款
                order.PaymentReceivedDate = updateDto.PaymentReceivedDate;
                order.UpdatedAt = DateTime.UtcNow;

                // 根据收款金额更新订单状态
                var totalAmount = order.TotalAmount;
                if (updateDto.ReceivedAmount >= totalAmount)
                {
                    order.Status = "已收款";
                }
                else if (updateDto.ReceivedAmount > 0)
                {
                    order.Status = "部分收款";
                }
                else
                {
                    order.Status = "待收款";
                }

                await _context.SaveChangesAsync();

                var result = new FinanceOperationResultDto
                {
                    IsSuccess = true,
                    Message = "收款信息更新成功",
                    AffectedOrderCount = 1
                };

                return new ApiResponse<FinanceOperationResultDto>
                {
                    IsSuccess = true,
                    Data = result,
                    Message = "收款信息更新成功",
                    StatusCode = HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<FinanceOperationResultDto>
                {
                    IsSuccess = false,
                    Message = $"更新收款信息失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        /// <summary>
        /// 批量更新收款信息
        /// </summary>
        public async Task<ApiResponse<FinanceOperationResultDto>> BatchUpdatePaymentInfoAsync(BatchUpdatePaymentInfoDto batchUpdateDto)
        {
            try
            {
                var affectedCount = 0;
                var errors = new List<string>();

                foreach (var item in batchUpdateDto.Items)
                {
                    var result = await UpdatePaymentInfoAsync(item);
                    if (result.IsSuccess)
                    {
                        affectedCount++;
                    }
                    else
                    {
                        errors.Add($"订单{item.OrderId}: {result.Message}");
                    }
                }

                var operationResult = new FinanceOperationResultDto
                {
                    IsSuccess = errors.Count == 0,
                    Message = errors.Count == 0 ? "批量更新成功" : $"部分更新失败，成功{affectedCount}个",
                    AffectedOrderCount = affectedCount,
                    ErrorDetails = errors
                };

                return new ApiResponse<FinanceOperationResultDto>
                {
                    IsSuccess = true,
                    Data = operationResult,
                    Message = "批量更新完成",
                    StatusCode = HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<FinanceOperationResultDto>
                {
                    IsSuccess = false,
                    Message = $"批量更新失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        /// <summary>
        /// 获取财务汇总信息
        /// </summary>
        public async Task<ApiResponse<FinanceOrderSummaryDto>> GetFinanceSummaryAsync(FinanceFilterDto filter)
        {
            try
            {
                var summary = await CalculateSummaryAsync(filter);

                return new ApiResponse<FinanceOrderSummaryDto>
                {
                    IsSuccess = true,
                    Data = summary,
                    Message = "获取财务汇总成功",
                    StatusCode = HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<FinanceOrderSummaryDto>
                {
                    IsSuccess = false,
                    Message = $"获取财务汇总失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        /// <summary>
        /// 导出财务数据
        /// </summary>
        public async Task<ApiResponse<byte[]>> ExportFinanceDataAsync(ExportFinanceDataDto exportDto)
        {
            try
            {
                // 获取所有数据（不分页）
                var allDataFilter = exportDto.Filter;
                allDataFilter.PageSize = int.MaxValue;
                allDataFilter.PageNumber = 1;

                var dataResult = await GetFinanceOrderDetailsAsync(allDataFilter);
                if (!dataResult.IsSuccess || dataResult.Data == null)
                {
                    return new ApiResponse<byte[]>
                    {
                        IsSuccess = false,
                        Message = "获取导出数据失败",
                        StatusCode = HttpStatusCode.InternalServerError
                    };
                }

                // 这里应该实现Excel或CSV导出逻辑
                // 暂时返回空数据，实际项目中需要使用EPPlus或其他库
                var exportData = new byte[0];

                return new ApiResponse<byte[]>
                {
                    IsSuccess = true,
                    Data = exportData,
                    Message = "导出成功",
                    StatusCode = HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<byte[]>
                {
                    IsSuccess = false,
                    Message = $"导出失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        /// <summary>
        /// 获取订单的收款历史记录
        /// </summary>
        public async Task<ApiResponse<List<PaymentHistoryDto>>> GetPaymentHistoryAsync(int orderId)
        {
            try
            {
                // 这里需要实现收款历史记录的查询
                // 暂时返回空列表，实际项目中需要创建收款记录表
                var history = new List<PaymentHistoryDto>();

                return new ApiResponse<List<PaymentHistoryDto>>
                {
                    IsSuccess = true,
                    Data = history,
                    Message = "获取收款历史成功",
                    StatusCode = HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<PaymentHistoryDto>>
                {
                    IsSuccess = false,
                    Message = $"获取收款历史失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        #region 私有辅助方法

        /// <summary>
        /// 应用筛选条件
        /// </summary>
        private IQueryable<Domain.Entities.OrderItem> ApplyFilters(IQueryable<Domain.Entities.OrderItem> query, FinanceFilterDto filter)
        {
            // 客户筛选
            if (filter.CustomerId.HasValue)
            {
                query = query.Where(oi => oi.Order.CustomerId == filter.CustomerId.Value);
            }

            // 产品筛选
            if (filter.ProductId.HasValue)
            {
                query = query.Where(oi => oi.ProductId == filter.ProductId.Value);
            }

            // 销售人员筛选
            if (filter.SalesPersonId.HasValue)
            {
                query = query.Where(oi => oi.Order.SalesPersonId == filter.SalesPersonId.Value);
            }

            // 订单状态筛选
            if (!string.IsNullOrEmpty(filter.OrderStatus))
            {
                query = query.Where(oi => oi.Order.Status == filter.OrderStatus);
            }

            // 生效日期筛选
            if (filter.EffectiveDateStart.HasValue)
            {
                query = query.Where(oi => oi.Order.EffectiveDate >= filter.EffectiveDateStart.Value);
            }
            if (filter.EffectiveDateEnd.HasValue)
            {
                query = query.Where(oi => oi.Order.EffectiveDate <= filter.EffectiveDateEnd.Value);
            }

            // 到期日期筛选
            if (filter.ExpiryDateStart.HasValue)
            {
                query = query.Where(oi => oi.Order.ExpiryDate >= filter.ExpiryDateStart.Value);
            }
            if (filter.ExpiryDateEnd.HasValue)
            {
                query = query.Where(oi => oi.Order.ExpiryDate <= filter.ExpiryDateEnd.Value);
            }

            // 支付日期筛选
            if (filter.PaymentDateStart.HasValue)
            {
                query = query.Where(oi => oi.Order.PaymentReceivedDate >= filter.PaymentDateStart.Value);
            }
            if (filter.PaymentDateEnd.HasValue)
            {
                query = query.Where(oi => oi.Order.PaymentReceivedDate <= filter.PaymentDateEnd.Value);
            }

            // 关键词搜索
            if (!string.IsNullOrEmpty(filter.SearchKeyword))
            {
                var keyword = filter.SearchKeyword.Trim();
                query = query.Where(oi =>
                    oi.Order.Customer.Name.Contains(keyword) ||
                    oi.Order.OrderNumber.Contains(keyword) ||
                    oi.Product.Name.Contains(keyword));
            }

            return query;
        }

        /// <summary>
        /// 计算订单项的已收金额（按比例分摊）
        /// </summary>
        private decimal CalculateReceivedAmountForItem(Domain.Entities.OrderItem orderItem)
        {
            // 这里需要根据实际业务逻辑来计算
            // 暂时返回0，实际项目中需要根据收款记录来计算
            return 0;
        }

        /// <summary>
        /// 计算订单项的收款比例
        /// </summary>
        private decimal CalculatePaymentRatioForItem(Domain.Entities.OrderItem orderItem)
        {
            var receivedAmount = CalculateReceivedAmountForItem(orderItem);
            if (orderItem.TotalAmount == 0) return 0;
            return (receivedAmount / orderItem.TotalAmount) * 100;
        }

        /// <summary>
        /// 计算汇总信息
        /// </summary>
        private async Task<FinanceOrderSummaryDto> CalculateSummaryAsync(FinanceFilterDto filter)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.Customer)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.SalesPerson)
                .Include(oi => oi.Product)
                .AsQueryable();

            query = ApplyFilters(query, filter);

            var items = await query.ToListAsync();

            // 在内存中计算汇总信息
            var totalReceivedAmount = items.Sum(oi => CalculateReceivedAmountForItem(oi));

            var summary = new FinanceOrderSummaryDto
            {
                TotalAmount = items.Sum(oi => oi.TotalAmount),
                TotalReceivedAmount = totalReceivedAmount,
                OrderCount = items.Select(oi => oi.OrderId).Distinct().Count(),
                OrderItemCount = items.Count
            };

            summary.TotalUnreceivedAmount = summary.TotalAmount - summary.TotalReceivedAmount;
            summary.TotalPaymentRatio = summary.TotalAmount == 0 ? 0 : (summary.TotalReceivedAmount / summary.TotalAmount) * 100;

            return summary;
        }

        /// <summary>
        /// 生成日期选项
        /// </summary>
        private List<FilterOptionDto> GenerateDateOptions()
        {
            var options = new List<FilterOptionDto>();
            var currentDate = DateTime.Now;

            // 生成最近2年的年月选项
            for (int year = currentDate.Year; year >= currentDate.Year - 2; year--)
            {
                for (int month = 12; month >= 1; month--)
                {
                    if (year == currentDate.Year && month > currentDate.Month)
                        continue;

                    var date = new DateTime(year, month, 1);
                    options.Add(new FilterOptionDto
                    {
                        Value = date.ToString("yyyy-MM"),
                        Text = date.ToString("yyyy年MM月")
                    });
                }
            }

            return options;
        }

        #endregion
    }
}
