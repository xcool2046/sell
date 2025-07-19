using Microsoft.EntityFrameworkCore;
using Sellsys.Application.DTOs.Orders;
using Sellsys.Application.Interfaces;
using Sellsys.CrossCutting.Common;
using Sellsys.Domain.Entities;
using Sellsys.Infrastructure.Data;
using System.Net;

namespace Sellsys.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly SellsysDbContext _context;

        public OrderService(SellsysDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<OrderDto>> CreateOrderAsync(OrderUpsertDto orderDto)
        {
            // 验证客户存在
            var customer = await _context.Customers.FindAsync(orderDto.CustomerId);
            if (customer == null)
            {
                return new ApiResponse<OrderDto> { IsSuccess = false, Message = "客户不存在", StatusCode = HttpStatusCode.BadRequest };
            }

            // 验证销售人员存在
            var salesPerson = await _context.Employees.FindAsync(orderDto.SalesPersonId);
            if (salesPerson == null)
            {
                return new ApiResponse<OrderDto> { IsSuccess = false, Message = "销售人员不存在", StatusCode = HttpStatusCode.BadRequest };
            }

            // 生成订单号
            var orderNumber = await GenerateOrderNumberAsync();

            var order = new Order
            {
                OrderNumber = orderNumber,
                CustomerId = orderDto.CustomerId,
                EffectiveDate = orderDto.EffectiveDate,
                ExpiryDate = orderDto.ExpiryDate,
                Status = orderDto.Status,
                SalesPersonId = orderDto.SalesPersonId,
                PaymentReceivedDate = orderDto.PaymentReceivedDate,
                SalesCommissionAmount = orderDto.SalesCommissionAmount,
                SupervisorCommissionAmount = orderDto.SupervisorCommissionAmount,
                ManagerCommissionAmount = orderDto.ManagerCommissionAmount
            };

            // 添加订单项
            foreach (var itemDto in orderDto.OrderItems)
            {
                var product = await _context.Products.FindAsync(itemDto.ProductId);
                if (product == null)
                {
                    return new ApiResponse<OrderDto> { IsSuccess = false, Message = $"产品ID {itemDto.ProductId} 不存在", StatusCode = HttpStatusCode.BadRequest };
                }

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    ActualPrice = itemDto.ActualPrice,
                    Quantity = itemDto.Quantity,
                    TotalAmount = itemDto.TotalAmount
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 重新查询以获取关联数据
            var createdOrder = await GetOrderWithDetailsAsync(order.Id);
            return ApiResponse<OrderDto>.Success(createdOrder!);
        }

        public async Task<ApiResponse<List<OrderDto>>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.SalesPerson)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.Name,
                    EffectiveDate = o.EffectiveDate,
                    ExpiryDate = o.ExpiryDate,
                    Status = o.Status,
                    SalesPersonId = o.SalesPersonId,
                    SalesPersonName = o.SalesPerson.Name,
                    PaymentReceivedDate = o.PaymentReceivedDate,
                    SalesCommissionAmount = o.SalesCommissionAmount,
                    SupervisorCommissionAmount = o.SupervisorCommissionAmount,
                    ManagerCommissionAmount = o.ManagerCommissionAmount,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        OrderId = oi.OrderId,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        ProductSpecification = oi.Product.Specification,
                        ProductUnit = oi.Product.Unit,
                        ProductPrice = oi.Product.ListPrice,
                        ActualPrice = oi.ActualPrice,
                        Quantity = oi.Quantity,
                        TotalAmount = oi.TotalAmount,
                        CreatedAt = oi.CreatedAt
                    }).ToList()
                })
                .ToListAsync();

            return ApiResponse<List<OrderDto>>.Success(orders);
        }

        public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int id)
        {
            var order = await GetOrderWithDetailsAsync(id);
            if (order == null)
            {
                return new ApiResponse<OrderDto> { IsSuccess = false, Message = "订单不存在", StatusCode = HttpStatusCode.NotFound };
            }

            return ApiResponse<OrderDto>.Success(order);
        }

        public async Task<ApiResponse<List<OrderDto>>> SearchOrdersAsync(
            string? customerName = null,
            string? productName = null,
            DateTime? effectiveDateFrom = null,
            DateTime? effectiveDateTo = null,
            DateTime? expiryDateFrom = null,
            DateTime? expiryDateTo = null,
            DateTime? createdDateFrom = null,
            DateTime? createdDateTo = null,
            string? status = null,
            int? salesPersonId = null)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.SalesPerson)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(customerName))
            {
                query = query.Where(o => o.Customer.Name.Contains(customerName));
            }

            if (!string.IsNullOrWhiteSpace(productName))
            {
                query = query.Where(o => o.OrderItems.Any(oi => oi.Product.Name.Contains(productName)));
            }

            if (effectiveDateFrom.HasValue)
            {
                query = query.Where(o => o.EffectiveDate >= effectiveDateFrom.Value);
            }

            if (effectiveDateTo.HasValue)
            {
                query = query.Where(o => o.EffectiveDate <= effectiveDateTo.Value);
            }

            if (expiryDateFrom.HasValue)
            {
                query = query.Where(o => o.ExpiryDate >= expiryDateFrom.Value);
            }

            if (expiryDateTo.HasValue)
            {
                query = query.Where(o => o.ExpiryDate <= expiryDateTo.Value);
            }

            if (createdDateFrom.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= createdDateFrom.Value);
            }

            if (createdDateTo.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= createdDateTo.Value.AddDays(1));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (salesPersonId.HasValue)
            {
                query = query.Where(o => o.SalesPersonId == salesPersonId.Value);
            }

            var orders = await query
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.Name,
                    EffectiveDate = o.EffectiveDate,
                    ExpiryDate = o.ExpiryDate,
                    Status = o.Status,
                    SalesPersonId = o.SalesPersonId,
                    SalesPersonName = o.SalesPerson.Name,
                    PaymentReceivedDate = o.PaymentReceivedDate,
                    SalesCommissionAmount = o.SalesCommissionAmount,
                    SupervisorCommissionAmount = o.SupervisorCommissionAmount,
                    ManagerCommissionAmount = o.ManagerCommissionAmount,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        OrderId = oi.OrderId,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        ProductSpecification = oi.Product.Specification,
                        ProductUnit = oi.Product.Unit,
                        ProductPrice = oi.Product.ListPrice,
                        ActualPrice = oi.ActualPrice,
                        Quantity = oi.Quantity,
                        TotalAmount = oi.TotalAmount,
                        CreatedAt = oi.CreatedAt
                    }).ToList()
                })
                .ToListAsync();

            return ApiResponse<List<OrderDto>>.Success(orders);
        }

        public async Task<ApiResponse<OrderSummaryDto>> GetOrderSummaryAsync(List<int>? orderIds = null)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (orderIds != null && orderIds.Count > 0)
            {
                query = query.Where(o => orderIds.Contains(o.Id));
            }

            var orders = await query.ToListAsync();

            var summary = new OrderSummaryDto
            {
                TotalOrders = orders.Count,
                TotalAmount = orders.Sum(o => o.OrderItems.Sum(oi => oi.TotalAmount)),
                TotalQuantity = orders.Sum(o => o.OrderItems.Sum(oi => oi.Quantity)),
                StatusCounts = orders.GroupBy(o => o.Status)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            summary.AverageOrderAmount = summary.TotalOrders > 0
                ? summary.TotalAmount / summary.TotalOrders
                : 0;

            return ApiResponse<OrderSummaryDto>.Success(summary);
        }

        private async Task<OrderDto?> GetOrderWithDetailsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.SalesPerson)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.Id == id)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.Name,
                    EffectiveDate = o.EffectiveDate,
                    ExpiryDate = o.ExpiryDate,
                    Status = o.Status,
                    SalesPersonId = o.SalesPersonId,
                    SalesPersonName = o.SalesPerson.Name,
                    PaymentReceivedDate = o.PaymentReceivedDate,
                    SalesCommissionAmount = o.SalesCommissionAmount,
                    SupervisorCommissionAmount = o.SupervisorCommissionAmount,
                    ManagerCommissionAmount = o.ManagerCommissionAmount,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        OrderId = oi.OrderId,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        ProductSpecification = oi.Product.Specification,
                        ProductUnit = oi.Product.Unit,
                        ProductPrice = oi.Product.ListPrice,
                        ActualPrice = oi.ActualPrice,
                        Quantity = oi.Quantity,
                        TotalAmount = oi.TotalAmount,
                        CreatedAt = oi.CreatedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        private async Task<string> GenerateOrderNumberAsync()
        {
            var today = DateTime.Today;
            var prefix = $"ORD{today:yyyyMMdd}";
            
            var lastOrder = await _context.Orders
                .Where(o => o.OrderNumber.StartsWith(prefix))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefaultAsync();

            if (lastOrder == null)
            {
                return $"{prefix}001";
            }

            var lastNumber = lastOrder.OrderNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumber, out int number))
            {
                return $"{prefix}{(number + 1):D3}";
            }

            return $"{prefix}001";
        }

        public async Task<ApiResponse> DeleteOrderAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return ApiResponse.Fail("订单不存在", HttpStatusCode.NotFound);
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return ApiResponse.Success();
        }
    }
}
