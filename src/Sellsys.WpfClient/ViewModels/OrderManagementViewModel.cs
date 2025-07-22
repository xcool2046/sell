using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class OrderManagementViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Order> _orders;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Employee> _employees;
        private ObservableCollection<Product> _products;
        private Order? _selectedOrder;
        private OrderSummary? _orderSummary;
        private bool _isLoading;

        // 搜索和筛选属性
        private string _searchCustomerName = string.Empty;
        private string _searchProductName = string.Empty;
        private DateTime? _effectiveDateFrom;
        private DateTime? _effectiveDateTo;
        private DateTime? _expiryDateFrom;
        private DateTime? _expiryDateTo;
        private DateTime? _createdDateFrom;
        private DateTime? _createdDateTo;
        private string? _selectedStatus;
        private int? _selectedSalesPersonId;

        // 筛选选项
        private ObservableCollection<string> _statusOptions;
        private ObservableCollection<Employee> _salesPersonOptions;

        #region Properties

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set => SetProperty(ref _employees, value);
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        public OrderSummary? OrderSummary
        {
            get => _orderSummary;
            set => SetProperty(ref _orderSummary, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // 搜索和筛选属性
        public string SearchCustomerName
        {
            get => _searchCustomerName;
            set => SetProperty(ref _searchCustomerName, value);
        }

        public string SearchProductName
        {
            get => _searchProductName;
            set => SetProperty(ref _searchProductName, value);
        }

        public DateTime? EffectiveDateFrom
        {
            get => _effectiveDateFrom;
            set => SetProperty(ref _effectiveDateFrom, value);
        }

        public DateTime? EffectiveDateTo
        {
            get => _effectiveDateTo;
            set => SetProperty(ref _effectiveDateTo, value);
        }

        public DateTime? ExpiryDateFrom
        {
            get => _expiryDateFrom;
            set => SetProperty(ref _expiryDateFrom, value);
        }

        public DateTime? ExpiryDateTo
        {
            get => _expiryDateTo;
            set => SetProperty(ref _expiryDateTo, value);
        }

        public DateTime? CreatedDateFrom
        {
            get => _createdDateFrom;
            set => SetProperty(ref _createdDateFrom, value);
        }

        public DateTime? CreatedDateTo
        {
            get => _createdDateTo;
            set => SetProperty(ref _createdDateTo, value);
        }

        public string? SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
        }

        public int? SelectedSalesPersonId
        {
            get => _selectedSalesPersonId;
            set => SetProperty(ref _selectedSalesPersonId, value);
        }

        // 筛选选项
        public ObservableCollection<string> StatusOptions
        {
            get => _statusOptions;
            set => SetProperty(ref _statusOptions, value);
        }

        public ObservableCollection<Employee> SalesPersonOptions
        {
            get => _salesPersonOptions;
            set => SetProperty(ref _salesPersonOptions, value);
        }

        #endregion

        #region Commands

        public ICommand LoadDataCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand ViewOrderDetailsCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        public OrderManagementViewModel()
        {
            System.Diagnostics.Debug.WriteLine("OrderManagementViewModel: Constructor called");

            _apiService = new ApiService();
            _orders = new ObservableCollection<Order>();
            _customers = new ObservableCollection<Customer>();
            _employees = new ObservableCollection<Employee>();
            _products = new ObservableCollection<Product>();
            _statusOptions = new ObservableCollection<string>();
            _salesPersonOptions = new ObservableCollection<Employee>();

            // Initialize commands
            LoadDataCommand = new AsyncRelayCommand(async p => await LoadDataAsync());
            SearchCommand = new AsyncRelayCommand(async p => await SearchOrdersAsync());
            ResetFiltersCommand = new RelayCommand(p => ResetFilters());
            ViewOrderDetailsCommand = new RelayCommand(p => ViewOrderDetails(), p => SelectedOrder != null);
            RefreshCommand = new AsyncRelayCommand(async p => await LoadOrdersAsync());

            // Initialize filter data
            InitializeFilterData();
        }

        public override async Task LoadDataAsync()
        {
            System.Diagnostics.Debug.WriteLine("OrderManagementViewModel: LoadDataAsync called");

            if (IsDataLoaded)
            {
                System.Diagnostics.Debug.WriteLine("OrderManagementViewModel: Data already loaded, skipping");
                return; // Avoid loading data multiple times
            }

            await LoadOrdersAsync();
            await LoadReferenceDataAsync();
            IsDataLoaded = true;

            System.Diagnostics.Debug.WriteLine("OrderManagementViewModel: LoadDataAsync completed");
        }

        private void InitializeFilterData()
        {
            // Initialize status options
            StatusOptions.Clear();
            StatusOptions.Add("全部");
            StatusOptions.Add("待收款");
            StatusOptions.Add("已收款");
            StatusOptions.Add("已完成");
            StatusOptions.Add("已取消");

            // Set default values
            SelectedStatus = "全部";
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                IsLoading = true;

                var orders = await _apiService.GetOrdersAsync();
                
                Orders.Clear();
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }

                // Update summary
                await UpdateOrderSummaryAsync();
            }
            catch (Exception ex)
            {
                ErrorHandlingService.HandleApiError(ex, "loading order data");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadReferenceDataAsync()
        {
            try
            {
                // Load customers, employees, and products for dropdowns
                var customersTask = _apiService.GetCustomersAsync();
                var employeesTask = _apiService.GetEmployeesAsync();
                var productsTask = _apiService.GetProductsAsync();

                await Task.WhenAll(customersTask, employeesTask, productsTask);

                // Update collections
                Customers.Clear();
                foreach (var customer in customersTask.Result)
                {
                    Customers.Add(customer);
                }

                Employees.Clear();
                SalesPersonOptions.Clear();
                SalesPersonOptions.Add(new Employee { Id = 0, Name = "全部" }); // Add "All" option
                foreach (var employee in employeesTask.Result)
                {
                    Employees.Add(employee);
                    SalesPersonOptions.Add(employee);
                }

                Products.Clear();
                if (productsTask.Result != null)
                {
                    foreach (var product in productsTask.Result)
                    {
                        Products.Add(product);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandlingService.HandleApiError(ex, "loading reference data");
            }
        }

        private async Task SearchOrdersAsync()
        {
            try
            {
                IsLoading = true;

                var criteria = new OrderSearchCriteria
                {
                    CustomerName = string.IsNullOrWhiteSpace(SearchCustomerName) ? null : SearchCustomerName,
                    ProductName = string.IsNullOrWhiteSpace(SearchProductName) ? null : SearchProductName,
                    EffectiveDateFrom = EffectiveDateFrom,
                    EffectiveDateTo = EffectiveDateTo,
                    ExpiryDateFrom = ExpiryDateFrom,
                    ExpiryDateTo = ExpiryDateTo,
                    CreatedDateFrom = CreatedDateFrom,
                    CreatedDateTo = CreatedDateTo,
                    Status = SelectedStatus == "全部" ? null : SelectedStatus,
                    SalesPersonId = SelectedSalesPersonId == 0 ? null : SelectedSalesPersonId
                };

                var orders = await _apiService.SearchOrdersAsync(criteria);

                Orders.Clear();
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }

                // Update summary for filtered results
                await UpdateOrderSummaryAsync();
            }
            catch (Exception ex)
            {
                ErrorHandlingService.HandleApiError(ex, "searching orders");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateOrderSummaryAsync()
        {
            try
            {
                var orderIds = Orders.Select(o => o.Id).ToList();
                if (orderIds.Count > 0)
                {
                    OrderSummary = await _apiService.GetOrderSummaryAsync(orderIds);
                }
                else
                {
                    OrderSummary = new OrderSummary();
                }
            }
            catch (Exception ex)
            {
                // Don't show error for summary calculation failure
                System.Diagnostics.Debug.WriteLine($"Error calculating order summary: {ex.Message}");
                OrderSummary = new OrderSummary();
            }
        }

        private void ResetFilters()
        {
            SearchCustomerName = string.Empty;
            SearchProductName = string.Empty;
            EffectiveDateFrom = null;
            EffectiveDateTo = null;
            ExpiryDateFrom = null;
            ExpiryDateTo = null;
            CreatedDateFrom = null;
            CreatedDateTo = null;
            SelectedStatus = "全部";
            SelectedSalesPersonId = 0;
        }

        private void AddOrder()
        {
            try
            {
                // TODO: Implement add order dialog
                MessageBox.Show("添加订单功能将在后续版本中实现", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开添加订单对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditOrder()
        {
            if (SelectedOrder == null) return;
            EditOrderRow(SelectedOrder);
        }

        private void EditOrderRow(Order? order)
        {
            if (order == null) return;

            try
            {
                // TODO: Implement edit order dialog
                MessageBox.Show($"编辑订单功能将在后续版本中实现\n订单号: {order.OrderNumber}", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开编辑订单对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteOrderAsync()
        {
            if (SelectedOrder == null) return;
            await DeleteOrderRowAsync(SelectedOrder);
        }

        private async Task DeleteOrderRowAsync(Order? order)
        {
            if (order == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"确定要删除订单 '{order.OrderNumber}' 吗？\n此操作不可撤销。",
                    "确认删除",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    IsLoading = true;
                    await _apiService.DeleteOrderAsync(order.Id);
                    await LoadOrdersAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorHandlingService.HandleApiError(ex, "deleting order");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ViewOrderDetails()
        {
            if (SelectedOrder == null) return;
            ViewOrderDetailsRow(SelectedOrder);
        }

        private void ViewOrderDetailsRow(Order? order)
        {
            if (order == null) return;

            try
            {
                // TODO: Implement order details dialog
                MessageBox.Show($"订单详情功能将在后续版本中实现\n订单号: {order.OrderNumber}", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开订单详情对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void UpdateOrderSummarySync()
        {
            if (OrderSummary == null)
                OrderSummary = new OrderSummary();

            OrderSummary.TotalOrders = Orders.Count;
            OrderSummary.TotalAmount = Orders.Sum(o => o.TotalAmount);
        }
    }
}
