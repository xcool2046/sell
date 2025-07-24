using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using Sellsys.WpfClient.ViewModels.Dialogs;
using Sellsys.WpfClient.Views.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Sellsys.WpfClient.ViewModels
{
    public class AfterSalesViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private ObservableCollection<CustomerAfterSales> _customerAfterSales;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Employee> _employees;
        private CustomerAfterSales? _selectedCustomer;
        private bool _isLoading;

        // 搜索过滤字段
        private string? _selectedCustomerName;
        private string? _selectedCustomerService;
        private string? _selectedStatus;

        public ObservableCollection<CustomerAfterSales> CustomerAfterSales
        {
            get => _customerAfterSales;
            set => SetProperty(ref _customerAfterSales, value);
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



        public CustomerAfterSales? SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // 搜索过滤属性
        public string? SelectedCustomerName
        {
            get => _selectedCustomerName;
            set => SetProperty(ref _selectedCustomerName, value);
        }

        public string? SelectedCustomerService
        {
            get => _selectedCustomerService;
            set => SetProperty(ref _selectedCustomerService, value);
        }

        public string? SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
        }

        // 下拉框选项
        public ObservableCollection<string> CustomerNameOptions { get; } = new ObservableCollection<string>();

        // 客服选项 - will be loaded from API
        public ObservableCollection<string> CustomerServiceOptions { get; } = new ObservableCollection<string> { "全部" };

        // 状态选项
        public List<string> StatusOptions { get; } = new List<string>
        {
            "全部", "待处理", "处理中", "处理完成"
        };

        // Commands
        public ICommand LoadDataCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand ViewRecordsCommand { get; }
        public ICommand RefreshCommand { get; }

        // Row-level commands
        public ICommand ViewRecordsRowCommand { get; }
        public ICommand ViewServiceRecordsCommand { get; }

        public AfterSalesViewModel()
        {
            _apiService = new ApiService();
            _customerAfterSales = new ObservableCollection<CustomerAfterSales>();
            _customers = new ObservableCollection<Customer>();
            _employees = new ObservableCollection<Employee>();

            // Initialize commands
            LoadDataCommand = new AsyncRelayCommand(async p => await LoadAfterSalesDataAsync());
            SearchCommand = new AsyncRelayCommand(async p => await SearchRecordsAsync());
            ResetFiltersCommand = new RelayCommand(p => ResetFilters());
            ViewRecordsCommand = new RelayCommand(p => ViewRecords(), p => SelectedCustomer != null);
            RefreshCommand = new AsyncRelayCommand(async p => await LoadAfterSalesDataAsync());

            // Row-level commands
            ViewRecordsRowCommand = new RelayCommand(p => ViewRecordsRow(p as CustomerAfterSales), p => p is CustomerAfterSales);
            ViewServiceRecordsCommand = new RelayCommand(p => ViewServiceRecords(p as CustomerAfterSales), p => p is CustomerAfterSales);

            // 订阅客户更新事件
            EventBus.Instance.Subscribe<CustomerUpdatedEvent>(OnCustomerUpdated);
            EventBus.Instance.Subscribe<CustomerAssignedEvent>(OnCustomerAssigned);

            // 订阅员工更新事件
            EventBus.Instance.Subscribe<EmployeeUpdatedEvent>(OnEmployeeUpdated);

            // Note: Data loading is now triggered manually or when view becomes active
            // This prevents API calls during application startup
        }

        public override async Task LoadDataAsync()
        {
            if (IsDataLoaded) return; // Avoid loading data multiple times
            await LoadAfterSalesDataAsync();
            await LoadEmployeesForFilterAsync();
            IsDataLoaded = true;
        }

        private async Task LoadAfterSalesDataAsync()
        {
            try
            {
                IsLoading = true;

                // Load all required data
                var customerAfterSalesTask = _apiService.GetCustomersWithAfterSalesInfoAsync();
                var customersTask = _apiService.GetCustomersAsync();
                var employeesTask = _apiService.GetEmployeesAsync();

                await Task.WhenAll(customerAfterSalesTask, customersTask, employeesTask);

                // Update collections
                CustomerAfterSales.Clear();
                foreach (var customerAfterSales in customerAfterSalesTask.Result)
                {
                    // 应用分组权限过滤
                    if (ShouldShowCustomerAfterSales(customerAfterSales))
                    {
                        CustomerAfterSales.Add(customerAfterSales);
                    }
                }

                Customers.Clear();
                foreach (var customer in customersTask.Result)
                {
                    Customers.Add(customer);
                }

                Employees.Clear();
                foreach (var employee in employeesTask.Result)
                {
                    Employees.Add(employee);
                }

                // 初始化下拉框选项
                InitializeFilterOptions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void InitializeFilterOptions()
        {
            // 客户名称选项
            CustomerNameOptions.Clear();
            var customerNames = Customers.Select(c => c.Name).Distinct().OrderBy(n => n);
            foreach (var name in customerNames)
            {
                CustomerNameOptions.Add(name);
            }

        }

        private async Task SearchRecordsAsync()
        {
            try
            {
                IsLoading = true;

                // 使用后端搜索API
                var filteredCustomers = await _apiService.SearchCustomersWithAfterSalesInfoAsync(
                    customerName: SelectedCustomerName,
                    supportPersonName: SelectedCustomerService,
                    status: SelectedStatus
                );

                CustomerAfterSales.Clear();
                foreach (var customer in filteredCustomers)
                {
                    // 应用分组权限过滤
                    if (ShouldShowCustomerAfterSales(customer))
                    {
                        CustomerAfterSales.Add(customer);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"搜索客户售后信息失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ViewRecords()
        {
            if (SelectedCustomer == null) return;
            ViewRecordsRow(SelectedCustomer);
        }

        private void ViewRecordsRow(CustomerAfterSales? customerAfterSales)
        {
            if (customerAfterSales == null) return;

            // 找到对应的客户
            var customer = Customers.FirstOrDefault(c => c.Id == customerAfterSales.CustomerId);
            if (customer == null)
            {
                MessageBox.Show("未找到对应的客户信息", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 打开客服记录弹窗
            var dialogViewModel = new CustomerServiceRecordsDialogViewModel(_apiService, customer, Employees.ToList());
            var dialog = new CustomerServiceRecordsDialog(dialogViewModel);

            // 订阅记录变更事件，用于刷新主界面数据
            dialogViewModel.RecordChanged += async (sender, e) =>
            {
                await LoadAfterSalesDataAsync();
            };

            dialog.ShowDialog();
        }

        private void ResetFilters()
        {
            SelectedCustomerName = null;
            SelectedCustomerService = null;
            SelectedStatus = null;
        }

        private void ViewServiceRecords(CustomerAfterSales? customerAfterSales)
        {
            if (customerAfterSales == null) return;

            // 使用与ViewRecordsRow相同的逻辑
            ViewRecordsRow(customerAfterSales);
        }

        private async Task LoadEmployeesForFilterAsync()
        {
            try
            {
                var employees = await _apiService.GetEmployeesAsync();

                // Clear existing customer service options except "全部"
                CustomerServiceOptions.Clear();
                CustomerServiceOptions.Add("全部");

                // Add employees to customer service filter (filter for customer service roles if needed)
                foreach (var employee in employees.Where(e => e.RoleName?.Contains("客服") == true || e.RoleName?.Contains("服务") == true))
                {
                    CustomerServiceOptions.Add(employee.Name);
                }

                // If no customer service employees found, add all employees
                if (CustomerServiceOptions.Count == 1)
                {
                    foreach (var employee in employees)
                    {
                        CustomerServiceOptions.Add(employee.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the whole operation
                System.Diagnostics.Debug.WriteLine($"Error loading employees for filter: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理客户更新事件
        /// </summary>
        private async void OnCustomerUpdated(CustomerUpdatedEvent eventData)
        {
            try
            {
                // 如果数据已加载，则刷新数据以显示最新的客户信息
                if (IsDataLoaded)
                {
                    System.Diagnostics.Debug.WriteLine($"AfterSales: 收到客户更新事件 - {eventData.UpdateType}: {eventData.CustomerName}");

                    // 重置数据加载标志，强制重新加载
                    IsDataLoaded = false;
                    await LoadAfterSalesDataAsync();
                    await LoadEmployeesForFilterAsync();
                    IsDataLoaded = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"处理客户更新事件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理客户分配事件
        /// </summary>
        private async void OnCustomerAssigned(CustomerAssignedEvent eventData)
        {
            try
            {
                // 如果数据已加载，则刷新数据以显示最新的分配信息
                if (IsDataLoaded)
                {
                    System.Diagnostics.Debug.WriteLine($"AfterSales: 收到客户分配事件 - {eventData.AssignmentType}: {eventData.CustomerName}");

                    // 重置数据加载标志，强制重新加载
                    IsDataLoaded = false;
                    await LoadAfterSalesDataAsync();
                    await LoadEmployeesForFilterAsync();
                    IsDataLoaded = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"处理客户分配事件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理员工更新事件
        /// </summary>
        private async void OnEmployeeUpdated(EmployeeUpdatedEvent eventData)
        {
            try
            {
                // 如果数据已加载，则刷新数据以显示最新的员工信息
                if (IsDataLoaded)
                {
                    System.Diagnostics.Debug.WriteLine($"AfterSales: 收到员工更新事件 - {eventData.UpdateType}: {eventData.EmployeeName}");

                    // 重置数据加载标志，强制重新加载
                    IsDataLoaded = false;
                    await LoadAfterSalesDataAsync();
                    await LoadEmployeesForFilterAsync();
                    IsDataLoaded = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"处理员工更新事件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查是否应该显示指定客户的售后信息（基于分组权限）
        /// </summary>
        /// <param name="customerAfterSales">客户售后信息</param>
        /// <returns>是否应该显示</returns>
        private bool ShouldShowCustomerAfterSales(CustomerAfterSales customerAfterSales)
        {
            // 获取客服人员的分组信息
            var supportPersonGroupId = GetEmployeeGroupId(customerAfterSales.SupportPersonId);

            // 检查是否可以访问该客户的数据
            return CurrentUser.CanAccessUserData(customerAfterSales.SupportPersonId, supportPersonGroupId);
        }

        /// <summary>
        /// 获取员工的分组ID
        /// </summary>
        /// <param name="employeeId">员工ID</param>
        /// <returns>分组ID</returns>
        private int? GetEmployeeGroupId(int? employeeId)
        {
            if (!employeeId.HasValue)
                return null;

            var employee = Employees.FirstOrDefault(e => e.Id == employeeId.Value);
            return employee?.GroupId;
        }
    }
}
