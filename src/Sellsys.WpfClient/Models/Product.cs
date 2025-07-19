using System;

namespace Sellsys.WpfClient.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Specification { get; set; }
        public string? Unit { get; set; }
        public decimal ListPrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal? SalesCommission { get; set; }
        public decimal? SupervisorCommission { get; set; }
        public decimal? ManagerCommission { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Display properties for UI
        public string FormattedListPrice => ListPrice.ToString("C");
        public string FormattedMinPrice => MinPrice.ToString("C");
        public string FormattedSalesCommission => SalesCommission?.ToString("C") ?? "-";
        public string FormattedSupervisorCommission => SupervisorCommission?.ToString("C") ?? "-";
        public string FormattedManagerCommission => ManagerCommission?.ToString("C") ?? "-";
        public string UnitDisplay => !string.IsNullOrEmpty(Unit) ? Unit : "-";
        public string SpecificationDisplay => !string.IsNullOrEmpty(Specification) ? Specification : "-";
    }
}