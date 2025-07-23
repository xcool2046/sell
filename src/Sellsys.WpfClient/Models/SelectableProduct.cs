using Sellsys.WpfClient.ViewModels;
using System;

namespace Sellsys.WpfClient.Models
{
    public class SelectableProduct : ViewModelBase
    {
        private bool _isSelected;

        public SelectableProduct(Product product)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
        }

        public Product Product { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        // Delegate properties from Product for easy binding
        public int Id => Product.Id;
        public string Name => Product.Name;
        public string? Specification => Product.Specification;
        public string? Unit => Product.Unit;
        public decimal ListPrice => Product.ListPrice;
        public decimal MinPrice => Product.MinPrice;
        public decimal? SalesCommission => Product.SalesCommission;
        public decimal? SupervisorCommission => Product.SupervisorCommission;
        public decimal? ManagerCommission => Product.ManagerCommission;
        public DateTime CreatedAt => Product.CreatedAt;
        public DateTime UpdatedAt => Product.UpdatedAt;

        // Display properties
        public string FormattedListPrice => Product.FormattedListPrice;
        public string FormattedMinPrice => Product.FormattedMinPrice;
        public string FormattedSalesCommission => Product.FormattedSalesCommission;
        public string FormattedSupervisorCommission => Product.FormattedSupervisorCommission;
        public string FormattedManagerCommission => Product.FormattedManagerCommission;
        public string UnitDisplay => Product.UnitDisplay;
        public string SpecificationDisplay => Product.SpecificationDisplay;
    }
}
