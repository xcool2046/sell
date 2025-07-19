using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using Sellsys.WpfClient.ViewModels.Dialogs;
using Sellsys.WpfClient.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class AfterSalesViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private ObservableCollection<AfterSalesRecord> _afterSalesRecords;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Employee> _employees;
        private AfterSalesRecord? _selectedRecord;
        private bool _isLoading;

        // 搜索过滤字段
        private string? _selectedCustomerName;
        private string? _selectedCustomerService;
        private string? _selectedStatus;

        public ObservableCollection<AfterSalesRecord> AfterSalesRecords
        {
            get => _afterSalesRecords;
            set => SetProperty(ref _afterSalesRecords, value);
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



        public AfterSalesRecord? SelectedRecord
        {
            get => _selectedRecord;
            set => SetProperty(ref _selectedRecord, value);
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

        // 客服选项
        public List<string> CustomerServiceOptions { get; } = new List<string>
        {
            "全部", "客服A", "客服B", "客服C"
        };

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

        public AfterSalesViewModel()
        {
            _apiService = new ApiService();
            _afterSalesRecords = new ObservableCollection<AfterSalesRecord>();
            _customers = new ObservableCollection<Customer>();
            _employees = new ObservableCollection<Employee>();

            // Initialize commands
            LoadDataCommand = new AsyncRelayCommand(async p => await LoadAfterSalesDataAsync());
            SearchCommand = new AsyncRelayCommand(async p => await SearchRecordsAsync());
            ResetFiltersCommand = new RelayCommand(p => ResetFilters());
            ViewRecordsCommand = new RelayCommand(p => ViewRecords(), p => SelectedRecord != null);
            RefreshCommand = new AsyncRelayCommand(async p => await LoadAfterSalesDataAsync());

            // Row-level commands
            ViewRecordsRowCommand = new RelayCommand(p => ViewRecordsRow(p as AfterSalesRecord), p => p is AfterSalesRecord);

            // Note: Data loading is now triggered manually or when view becomes active
            // This prevents API calls during application startup
        }

        public override async Task LoadDataAsync()
        {
            if (IsDataLoaded) return; // Avoid loading data multiple times
            await LoadAfterSalesDataAsync();
            IsDataLoaded = true;
        }

        private async Task LoadAfterSalesDataAsync()
        {
            try
            {
                IsLoading = true;

                // Load all required data
                var recordsTask = _apiService.GetAfterSalesRecordsAsync();
                var customersTask = _apiService.GetCustomersAsync();
                var employeesTask = _apiService.GetEmployeesAsync();

                await Task.WhenAll(recordsTask, customersTask, employeesTask);

                // Update collections
                AfterSalesRecords.Clear();
                foreach (var record in recordsTask.Result.OrderByDescending(r => r.CreatedAt))
                {
                    AfterSalesRecords.Add(record);
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
                var filteredRecords = await _apiService.SearchAfterSalesRecordsAsync(
                    customerName: SelectedCustomerName,
                    province: null,
                    city: null,
                    status: SelectedStatus
                );

                AfterSalesRecords.Clear();
                foreach (var record in filteredRecords.OrderByDescending(r => r.CreatedAt))
                {
                    AfterSalesRecords.Add(record);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"搜索售后记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ViewRecords()
        {
            if (SelectedRecord == null) return;
            ViewRecordsRow(SelectedRecord);
        }

        private void ViewRecordsRow(AfterSalesRecord? record)
        {
            if (record == null) return;

            // 找到对应的客户
            var customer = Customers.FirstOrDefault(c => c.Id == record.CustomerId);
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
    }
}
