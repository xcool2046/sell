using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class SelectProductDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private ObservableCollection<SelectableProduct> _products;
        private SelectableProduct? _selectedProduct;
        private bool _isLoading;

        public SelectProductDialogViewModel()
        {
            _apiService = new ApiService();
            _products = new ObservableCollection<SelectableProduct>();

            // Initialize commands
            CancelCommand = new RelayCommand(p => Cancel());
            ConfirmCommand = new RelayCommand(p => Confirm(), p => CanConfirm());

            // Load products
            _ = LoadProductsAsync();
        }

        public ObservableCollection<SelectableProduct> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public SelectableProduct? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    // Update selection state
                    UpdateSelection(value);
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Commands
        public ICommand CancelCommand { get; }
        public ICommand ConfirmCommand { get; }

        // Events
        public event EventHandler? CloseRequested;
        public event EventHandler<Product>? ProductSelected;

        private async Task LoadProductsAsync()
        {
            try
            {
                IsLoading = true;
                
                var products = await _apiService.GetProductsAsync();
                
                Products.Clear();
                foreach (var product in products)
                {
                    var selectableProduct = new SelectableProduct(product);
                    
                    // Subscribe to selection changes
                    selectableProduct.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName == nameof(SelectableProduct.IsSelected) && sender is SelectableProduct sp && sp.IsSelected)
                        {
                            // Ensure only one product is selected
                            foreach (var p in Products.Where(x => x != sp))
                            {
                                p.IsSelected = false;
                            }
                            SelectedProduct = sp;
                        }
                    };
                    
                    Products.Add(selectableProduct);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载产品列表失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateSelection(SelectableProduct? selectedProduct)
        {
            // Clear all selections first
            foreach (var product in Products)
            {
                product.IsSelected = false;
            }

            // Set the selected product
            if (selectedProduct != null)
            {
                selectedProduct.IsSelected = true;
            }
        }

        private bool CanConfirm()
        {
            return Products.Any(p => p.IsSelected);
        }

        private void Confirm()
        {
            try
            {
                var selectedProduct = Products.FirstOrDefault(p => p.IsSelected);
                if (selectedProduct == null)
                {
                    MessageBox.Show("请选择一个产品", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ProductSelected?.Invoke(this, selectedProduct.Product);
                Cancel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"确认选择失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
