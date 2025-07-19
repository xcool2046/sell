using System;
using System.Collections.Generic;

namespace Sellsys.Application.DTOs.Sales
{
    public class SalesOrderDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<SalesOrderItemDto> Items { get; set; } = new List<SalesOrderItemDto>();
    }
}