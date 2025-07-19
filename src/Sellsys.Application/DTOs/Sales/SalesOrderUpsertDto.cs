using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sellsys.Application.DTOs.Sales
{
    public class SalesOrderUpsertDto
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public List<SalesOrderItemUpsertDto> Items { get; set; } = new List<SalesOrderItemUpsertDto>();
    }

    public class SalesOrderItemUpsertDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}