using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Sellsys.WpfClient.ViewModels
{
    public class ProductManagementViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private Product? _selectedProduct;
        private string _searchText = string.Empty;
        private bool _isLoading;

        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Commands
        public ICommand LoadProductsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand RefreshCommand { get; }

        // Row-level commands
        public ICommand EditProductRowCommand { get; }
        public ICommand DeleteProductRowCommand { get; }

        public ProductManagementViewModel()
        {
            _apiService = new ApiService();
            LoadProductsCommand = new AsyncRelayCommand(async (p) => await LoadProductsAsync());
            SearchCommand = new AsyncRelayCommand(async p => await SearchProductsAsync());
            ResetCommand = new RelayCommand(p => ResetFilters());
            AddProductCommand = new RelayCommand(p => AddProduct());
            EditProductCommand = new RelayCommand(p => EditProduct(), p => SelectedProduct != null);
            DeleteProductCommand = new AsyncRelayCommand(async (p) => await DeleteProductAsync(), (p) => CanDeleteProduct());
            RefreshCommand = new AsyncRelayCommand(async p => await LoadProductsAsync());

            // Row-level commands
            EditProductRowCommand = new RelayCommand(p => EditProductRow(p as Product));
            DeleteProductRowCommand = new AsyncRelayCommand(async p => await DeleteProductRowAsync(p as Product));

            // Note: Data loading is now triggered manually or when view becomes active
            // This prevents API calls during application startup
        }

        public override async Task LoadDataAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ProductManagementViewModel: LoadDataAsync called");
                if (IsDataLoaded)
                {
                    System.Diagnostics.Debug.WriteLine("ProductManagementViewModel: Data already loaded, skipping");
                    return; // Avoid loading data multiple times
                }

                System.Diagnostics.Debug.WriteLine("ProductManagementViewModel: Starting to load products");
                await LoadProductsAsync();
                IsDataLoaded = true;
                System.Diagnostics.Debug.WriteLine($"ProductManagementViewModel: Data loaded successfully, product count: {Products.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProductManagementViewModel: Error in LoadDataAsync: {ex.Message}");
                MessageBox.Show($"加载产品数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ProductManagementViewModel: LoadProductsAsync started");
                IsLoading = true;

                // 从API加载数据
                System.Diagnostics.Debug.WriteLine("ProductManagementViewModel: Calling API to get products");
                var products = await _apiService.GetProductsAsync();

                System.Diagnostics.Debug.WriteLine($"ProductManagementViewModel: API returned {products?.Count ?? 0} products");

                Products.Clear();
                if (products != null)
                {
                    foreach (var product in products)
                    {
                        Products.Add(product);
                    }
                    System.Diagnostics.Debug.WriteLine($"ProductManagementViewModel: Added {products.Count} products to collection");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ProductManagementViewModel: No products returned from API");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProductManagementViewModel: Exception in LoadProductsAsync: {ex.Message}");
                MessageBox.Show($"加载产品数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("ProductManagementViewModel: LoadProductsAsync completed");
            }
        }

        private async Task SearchProductsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadProductsAsync();
                return;
            }

            try
            {
                IsLoading = true;
                var allProducts = await _apiService.GetProductsAsync();
                if (allProducts != null)
                {
                    var filteredProducts = allProducts.Where(p =>
                        p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(p.Specification) && p.Specification.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    ).ToList();

                    Products.Clear();
                    foreach (var product in filteredProducts)
                    {
                        Products.Add(product);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"搜索产品失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddProduct()
        {
            try
            {
                var dialog = new Views.Dialogs.AddEditProductDialog();
                var viewModel = new ViewModels.Dialogs.AddEditProductDialogViewModel(_apiService);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.ProductSaved += async (sender, args) =>
                {
                    dialog.DialogResult = true;
                    await LoadProductsAsync();
                };

                viewModel.Cancelled += (sender, args) =>
                {
                    dialog.DialogResult = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开添加产品对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditProduct()
        {
            if (SelectedProduct == null) return;

            try
            {
                var dialog = new Views.Dialogs.AddEditProductDialog();
                var viewModel = new ViewModels.Dialogs.AddEditProductDialogViewModel(_apiService, SelectedProduct);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.ProductSaved += async (sender, args) =>
                {
                    dialog.DialogResult = true;
                    await LoadProductsAsync();
                };

                viewModel.Cancelled += (sender, args) =>
                {
                    dialog.DialogResult = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开编辑产品对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanDeleteProduct()
        {
            return SelectedProduct != null;
        }

        private void ResetFilters()
        {
            SearchText = string.Empty;
            _ = LoadProductsAsync();
        }

        private void EditProductRow(Product? product)
        {
            if (product == null) return;

            try
            {
                var dialog = new Views.Dialogs.AddEditProductDialog();
                var viewModel = new ViewModels.Dialogs.AddEditProductDialogViewModel(_apiService, product);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.ProductSaved += async (sender, args) =>
                {
                    dialog.DialogResult = true;
                    await LoadProductsAsync();
                };

                viewModel.Cancelled += (sender, args) =>
                {
                    dialog.DialogResult = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开编辑产品对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteProductRowAsync(Product? product)
        {
            if (product == null) return;

            var result = MessageBox.Show(
                $"确定要删除产品 '{product.Name}' 吗？此操作不可撤销。",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _apiService.DeleteProductAsync(product.Id);
                    Products.Remove(product);
                    MessageBox.Show("产品删除成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除产品失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null) return;

            var result = MessageBox.Show(
                $"确定要删除产品 '{SelectedProduct.Name}' 吗？此操作不可撤销。",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _apiService.DeleteProductAsync(SelectedProduct.Id);
                    Products.Remove(SelectedProduct);
                    MessageBox.Show("产品删除成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除产品失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }


    }
}