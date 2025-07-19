using Microsoft.EntityFrameworkCore;
using Sellsys.Application.DTOs.Sales;
using Sellsys.Application.Interfaces;
using Sellsys.CrossCutting.Common;
using Sellsys.Domain.Entities;
using Sellsys.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sellsys.Application.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly SellsysDbContext _context;

        public SalesOrderService(SellsysDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<SalesOrderDto>> CreateSalesOrderAsync(SalesOrderUpsertDto salesOrderDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var customer = await _context.Customers.FindAsync(salesOrderDto.CustomerId);
                if (customer == null)
                    return new ApiResponse<SalesOrderDto> { IsSuccess = false, Message = "Customer not found", StatusCode = HttpStatusCode.NotFound };

                var employee = await _context.Employees.FindAsync(salesOrderDto.EmployeeId);
                if (employee == null)
                    return new ApiResponse<SalesOrderDto> { IsSuccess = false, Message = "Employee not found", StatusCode = HttpStatusCode.NotFound };

                decimal totalAmount = 0;
                var orderItems = new List<SalesOrderItem>();

                foreach (var itemDto in salesOrderDto.Items)
                {
                    var product = await _context.Products.FindAsync(itemDto.ProductId);
                    if (product == null)
                        return new ApiResponse<SalesOrderDto> { IsSuccess = false, Message = $"Product with ID {itemDto.ProductId} not found", StatusCode = HttpStatusCode.NotFound };

                    if (product.StockQuantity < itemDto.Quantity)
                        return new ApiResponse<SalesOrderDto> { IsSuccess = false, Message = $"Not enough stock for product {product.Name}", StatusCode = HttpStatusCode.BadRequest };

                    product.StockQuantity -= itemDto.Quantity;
                    var itemTotal = product.Price * itemDto.Quantity;
                    totalAmount += itemTotal;

                    orderItems.Add(new SalesOrderItem
                    {
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = product.Price
                    });
                }

                var salesOrder = new SalesOrder
                {
                    OrderNumber = $"SO-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                    CustomerId = salesOrderDto.CustomerId,
                    SalespersonId = salesOrderDto.EmployeeId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Status = "Created",
                    OrderItems = orderItems
                };

                _context.SalesOrders.Add(salesOrder);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var resultDto = await GetSalesOrderByIdAsync(salesOrder.Id);
                return resultDto;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<SalesOrderDto> { IsSuccess = false, Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError };
            }
        }

        public async Task<ApiResponse> DeleteSalesOrderAsync(int id)
        {
            var salesOrder = await _context.SalesOrders.Include(s => s.OrderItems).FirstOrDefaultAsync(s => s.Id == id);
            if (salesOrder == null)
            {
                return ApiResponse.Fail("Sales order not found.", HttpStatusCode.NotFound);
            }
            
            // Revert stock quantity
            foreach (var item in salesOrder.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                }
            }
            
            _context.SalesOrders.Remove(salesOrder);
            await _context.SaveChangesAsync();
            return ApiResponse.Success();
        }

        public async Task<ApiResponse<List<SalesOrderDto>>> GetAllSalesOrdersAsync()
        {
            var salesOrders = await _context.SalesOrders
                .Include(s => s.Customer)
                .Include(s => s.Salesperson)
                .Select(s => new SalesOrderDto
                {
                    Id = s.Id,
                    CustomerId = s.CustomerId,
                    CustomerName = s.Customer.Name,
                    EmployeeId = s.SalespersonId,
                    EmployeeName = s.Salesperson.Name,
                    OrderDate = s.OrderDate,
                    TotalAmount = s.TotalAmount,
                    Status = s.Status,
                    CreatedAt = s.OrderDate
                })
                .ToListAsync();
            
            return ApiResponse<List<SalesOrderDto>>.Success(salesOrders);
        }

        public async Task<ApiResponse<SalesOrderDto>> GetSalesOrderByIdAsync(int id)
        {
            var salesOrder = await _context.SalesOrders
                .Include(s => s.Customer)
                .Include(s => s.Salesperson)
                .Include(s => s.OrderItems)
                    .ThenInclude(i => i.Product)
                .Where(s => s.Id == id)
                .Select(s => new SalesOrderDto
                {
                    Id = s.Id,
                    CustomerId = s.CustomerId,
                    CustomerName = s.Customer.Name,
                    EmployeeId = s.SalespersonId,
                    EmployeeName = s.Salesperson.Name,
                    OrderDate = s.OrderDate,
                    TotalAmount = s.TotalAmount,
                    Status = s.Status,
                    CreatedAt = s.OrderDate,
                    Items = s.OrderItems.Select(i => new SalesOrderItemDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.UnitPrice * i.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (salesOrder == null)
            {
                return new ApiResponse<SalesOrderDto> { IsSuccess = false, Message = "Sales Order not found", StatusCode = HttpStatusCode.NotFound };
            }

            return ApiResponse<SalesOrderDto>.Success(salesOrder);
        }
    }
}