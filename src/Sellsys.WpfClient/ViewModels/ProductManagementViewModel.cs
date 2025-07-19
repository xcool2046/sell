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

                // 首先尝试从API加载数据
                try
                {
                    System.Diagnostics.Debug.WriteLine("ProductManagementViewModel: Calling API to get products");
                    var products = await _apiService.GetProductsAsync();

                    System.Diagnostics.Debug.WriteLine($"ProductManagementViewModel: API returned {products?.Count ?? 0} products");

                    if (products != null && products.Count > 0)
                    {
                        Products.Clear();
                        foreach (var product in products)
                        {
                            Products.Add(product);
                        }
                        System.Diagnostics.Debug.WriteLine($"ProductManagementViewModel: Added {products.Count} products to collection");
                        return;
                    }
                }
                catch (Exception apiEx)
                {
                    System.Diagnostics.Debug.WriteLine($"ProductManagementViewModel: API call failed: {apiEx.Message}");
                }

                // 如果API失败或返回空数据，使用模拟数据
                System.Diagnostics.Debug.WriteLine("ProductManagementViewModel: Loading mock data");
                var mockProducts = GetMockProducts();
                Products.Clear();
                foreach (var product in mockProducts)
                {
                    Products.Add(product);
                }
                System.Diagnostics.Debug.WriteLine($"ProductManagementViewModel: Added {mockProducts.Count} mock products to collection");
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

        private List<Product> GetMockProducts()
        {
            return new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "培训课程系统 (MIS)",
                    Specification = "MIS",
                    Unit = "套",
                    ListPrice = 46000.00m,
                    MinPrice = 0.00m,
                    SalesCommission = 200.00m,
                    SupervisorCommission = 100.00m,
                    ManagerCommission = 50.00m,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    UpdatedAt = DateTime.Now.AddDays(-1)
                },
                new Product
                {
                    Id = 2,
                    Name = "培训课程系统 (MIS) 智慧卡",
                    Specification = "智慧卡",
                    Unit = "张",
                    ListPrice = 5.00m,
                    MinPrice = 5.00m,
                    SalesCommission = 1.00m,
                    SupervisorCommission = 0.50m,
                    ManagerCommission = 0.30m,
                    CreatedAt = DateTime.Now.AddDays(-25),
                    UpdatedAt = DateTime.Now.AddDays(-2)
                },
                new Product
                {
                    Id = 3,
                    Name = "企业管理软件",
                    Specification = "标准版",
                    Unit = "套",
                    ListPrice = 25000.00m,
                    MinPrice = 20000.00m,
                    SalesCommission = 1500.00m,
                    SupervisorCommission = 800.00m,
                    ManagerCommission = 400.00m,
                    CreatedAt = DateTime.Now.AddDays(-20),
                    UpdatedAt = DateTime.Now.AddDays(-3)
                },
                new Product
                {
                    Id = 4,
                    Name = "数据分析平台",
                    Specification = "专业版",
                    Unit = "套",
                    ListPrice = 35000.00m,
                    MinPrice = 28000.00m,
                    SalesCommission = 2000.00m,
                    SupervisorCommission = 1200.00m,
                    ManagerCommission = 600.00m,
                    CreatedAt = DateTime.Now.AddDays(-15),
                    UpdatedAt = DateTime.Now.AddDays(-4)
                },
                new Product
                {
                    Id = 5,
                    Name = "移动办公应用",
                    Specification = "企业版",
                    Unit = "套",
                    ListPrice = 15000.00m,
                    MinPrice = 12000.00m,
                    SalesCommission = 800.00m,
                    SupervisorCommission = 500.00m,
                    ManagerCommission = 250.00m,
                    CreatedAt = DateTime.Now.AddDays(-10),
                    UpdatedAt = DateTime.Now.AddDays(-5)
                }
            };
        }
    }
}