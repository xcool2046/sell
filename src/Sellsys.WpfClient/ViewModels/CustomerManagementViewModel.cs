using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using Sellsys.WpfClient.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class CustomerManagementViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Customer> _customers;
        private Customer? _selectedCustomer;
        private string _searchText = string.Empty;
        private bool _isLoading;

        // Filter properties
        private ObservableCollection<string> _industryTypes;
        private ObservableCollection<string> _provinces;
        private ObservableCollection<string> _cities;
        private ObservableCollection<string> _contactStatuses;
        private ObservableCollection<string> _responsiblePersons;

        private string? _selectedIndustryType;
        private string? _selectedProvince;
        private string? _selectedCity;
        private string? _selectedContactStatus;
        private string? _selectedResponsiblePerson;

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
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

        // Filter Properties
        public ObservableCollection<string> IndustryTypes
        {
            get => _industryTypes;
            set => SetProperty(ref _industryTypes, value);
        }

        public ObservableCollection<string> Provinces
        {
            get => _provinces;
            set => SetProperty(ref _provinces, value);
        }

        public ObservableCollection<string> Cities
        {
            get => _cities;
            set => SetProperty(ref _cities, value);
        }

        public ObservableCollection<string> ContactStatuses
        {
            get => _contactStatuses;
            set => SetProperty(ref _contactStatuses, value);
        }

        public ObservableCollection<string> ResponsiblePersons
        {
            get => _responsiblePersons;
            set => SetProperty(ref _responsiblePersons, value);
        }

        public string? SelectedIndustryType
        {
            get => _selectedIndustryType;
            set
            {
                if (SetProperty(ref _selectedIndustryType, value))
                {
                    _ = SearchCustomersAsync(); // Auto-search when filter changes
                }
            }
        }

        public string? SelectedProvince
        {
            get => _selectedProvince;
            set
            {
                if (SetProperty(ref _selectedProvince, value))
                {
                    _ = SearchCustomersAsync(); // Auto-search when filter changes
                }
            }
        }

        public string? SelectedCity
        {
            get => _selectedCity;
            set
            {
                if (SetProperty(ref _selectedCity, value))
                {
                    _ = SearchCustomersAsync(); // Auto-search when filter changes
                }
            }
        }

        public string? SelectedContactStatus
        {
            get => _selectedContactStatus;
            set
            {
                if (SetProperty(ref _selectedContactStatus, value))
                {
                    _ = SearchCustomersAsync(); // Auto-search when filter changes
                }
            }
        }

        public string? SelectedResponsiblePerson
        {
            get => _selectedResponsiblePerson;
            set
            {
                if (SetProperty(ref _selectedResponsiblePerson, value))
                {
                    _ = SearchCustomersAsync(); // Auto-search when filter changes
                }
            }
        }

        // Commands
        public ICommand LoadCustomersCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand AddCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AssignSalesCommand { get; }
        public ICommand AssignSupportCommand { get; }
        public ICommand ViewContactsCommand { get; }

        // Row-level commands (for DataGrid buttons)
        public ICommand EditCustomerRowCommand { get; }
        public ICommand DeleteCustomerRowCommand { get; }

        public CustomerManagementViewModel()
        {
            _apiService = new ApiService();
            _customers = new ObservableCollection<Customer>();

            // Initialize filter collections
            _industryTypes = new ObservableCollection<string>();
            _provinces = new ObservableCollection<string>();
            _cities = new ObservableCollection<string>();
            _contactStatuses = new ObservableCollection<string>();
            _responsiblePersons = new ObservableCollection<string>();

            // Initialize commands
            LoadCustomersCommand = new AsyncRelayCommand(async p => await LoadCustomersAsync());
            SearchCommand = new AsyncRelayCommand(async p => await SearchCustomersAsync());
            ResetFiltersCommand = new RelayCommand(p => ResetFilters());
            AddCustomerCommand = new RelayCommand(p => AddCustomer());
            EditCustomerCommand = new RelayCommand(p => EditCustomer(), p => SelectedCustomer != null);
            DeleteCustomerCommand = new AsyncRelayCommand(async p => await DeleteCustomerAsync(), p => SelectedCustomer != null);
            RefreshCommand = new AsyncRelayCommand(async p => await LoadCustomersAsync());
            AssignSalesCommand = new RelayCommand(p => AssignSales(), p => SelectedCustomer != null);
            AssignSupportCommand = new RelayCommand(p => AssignSupport(), p => SelectedCustomer != null);
            ViewContactsCommand = new RelayCommand(p => ViewContacts(p as Customer));

            // Row-level commands
            EditCustomerRowCommand = new RelayCommand(p => EditCustomerRow(p as Customer), p => p is Customer);
            DeleteCustomerRowCommand = new AsyncRelayCommand(async p => await DeleteCustomerRowAsync(p as Customer), p => p is Customer);

            // Initialize filter data
            InitializeFilterData();

            // Note: Data loading is now triggered manually or when view becomes active
            // This prevents API calls during application startup
        }

        public override async Task LoadDataAsync()
        {
            if (IsDataLoaded) return; // Avoid loading data multiple times
            await LoadCustomersAsync();
            IsDataLoaded = true;
        }

        private void InitializeFilterData()
        {
            // Initialize industry types
            IndustryTypes.Clear();
            IndustryTypes.Add("全部");
            IndustryTypes.Add("教育");
            IndustryTypes.Add("医疗");
            IndustryTypes.Add("制造业");
            IndustryTypes.Add("服务业");
            IndustryTypes.Add("科技");
            IndustryTypes.Add("金融");

            // Initialize provinces
            Provinces.Clear();
            Provinces.Add("全部");
            Provinces.Add("四川");
            Provinces.Add("广东");
            Provinces.Add("北京");
            Provinces.Add("上海");
            Provinces.Add("江苏");
            Provinces.Add("浙江");

            // Initialize cities
            Cities.Clear();
            Cities.Add("全部");
            Cities.Add("成都");
            Cities.Add("广州");
            Cities.Add("深圳");
            Cities.Add("北京");
            Cities.Add("上海");
            Cities.Add("南京");
            Cities.Add("杭州");

            // Initialize contact statuses
            ContactStatuses.Clear();
            ContactStatuses.Add("全部");
            ContactStatuses.Add("待分配");
            ContactStatuses.Add("跟进中");
            ContactStatuses.Add("已成交");
            ContactStatuses.Add("已流失");

            // Initialize responsible persons
            ResponsiblePersons.Clear();
            ResponsiblePersons.Add("全部");
            ResponsiblePersons.Add("张飞");
            ResponsiblePersons.Add("李逵");
            ResponsiblePersons.Add("陈小二");

            // Set default selections
            SelectedIndustryType = "全部";
            SelectedProvince = "全部";
            SelectedCity = "全部";
            SelectedContactStatus = "全部";
            SelectedResponsiblePerson = "全部";
        }

        private void ResetFilters()
        {
            SelectedIndustryType = "全部";
            SelectedProvince = "全部";
            SelectedCity = "全部";
            SelectedContactStatus = "全部";
            SelectedResponsiblePerson = "全部";
            SearchText = string.Empty;
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                IsLoading = true;

                // For now, use mock data since API might not be ready
                var customers = GetMockCustomers();

                Customers.Clear();
                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }

                // TODO: Replace with actual API call when backend is ready
                // var customers = await _apiService.GetCustomersAsync();
            }
            catch (Exception ex)
            {
                ErrorHandlingService.HandleApiError(ex, "loading customer data");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private List<Customer> GetMockCustomers()
        {
            return new List<Customer>
            {
                new Customer
                {
                    Id = 1,
                    Name = "示例客户公司",
                    IndustryTypes = "教育",
                    Province = "四川",
                    City = "成都",
                    Address = "成都市高新区示例地址",
                    Remarks = "这是一个示例客户",
                    SalesPersonName = "张飞",
                    CustomerIntention = "待分配",
                    PendingTasks = "无",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    Contacts = new System.Collections.ObjectModel.ObservableCollection<Contact>
                    {
                        new Contact { Id = 1, Name = "示例联系人", Phone = "13800138000", IsPrimary = true }
                    }
                }
            };
        }

        private async Task SearchCustomersAsync()
        {
            try
            {
                IsLoading = true;

                // Get all customers (using mock data for now)
                var allCustomers = GetMockCustomers();

                // Apply filters
                var filteredCustomers = allCustomers.AsEnumerable();

                // Industry type filter
                if (!string.IsNullOrEmpty(SelectedIndustryType) && SelectedIndustryType != "全部")
                {
                    filteredCustomers = filteredCustomers.Where(c =>
                        c.IndustryTypes?.Contains(SelectedIndustryType, StringComparison.OrdinalIgnoreCase) == true);
                }

                // Province filter
                if (!string.IsNullOrEmpty(SelectedProvince) && SelectedProvince != "全部")
                {
                    filteredCustomers = filteredCustomers.Where(c =>
                        c.Province?.Equals(SelectedProvince, StringComparison.OrdinalIgnoreCase) == true);
                }

                // City filter
                if (!string.IsNullOrEmpty(SelectedCity) && SelectedCity != "全部")
                {
                    filteredCustomers = filteredCustomers.Where(c =>
                        c.City?.Equals(SelectedCity, StringComparison.OrdinalIgnoreCase) == true);
                }

                // Contact status filter (based on customer intention)
                if (!string.IsNullOrEmpty(SelectedContactStatus) && SelectedContactStatus != "全部")
                {
                    filteredCustomers = filteredCustomers.Where(c =>
                        c.CustomerIntention?.Equals(SelectedContactStatus, StringComparison.OrdinalIgnoreCase) == true);
                }

                // Responsible person filter
                if (!string.IsNullOrEmpty(SelectedResponsiblePerson) && SelectedResponsiblePerson != "全部")
                {
                    filteredCustomers = filteredCustomers.Where(c =>
                        c.ResponsiblePersonName?.Equals(SelectedResponsiblePerson, StringComparison.OrdinalIgnoreCase) == true);
                }

                // Text search filter
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filteredCustomers = filteredCustomers.Where(c =>
                        c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        (c.Province?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (c.City?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (c.Address?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true));
                }

                // Update the customers collection
                Customers.Clear();
                foreach (var customer in filteredCustomers)
                {
                    Customers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"搜索客户失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddCustomer()
        {
            try
            {
                var dialog = new AddCustomerDialog();
                var viewModel = new AddCustomerDialogViewModel(_apiService);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.CustomerSaved += async (sender, args) =>
                {
                    dialog.DialogResult = true;
                    await LoadCustomersAsync();
                };

                viewModel.Cancelled += (sender, args) =>
                {
                    dialog.DialogResult = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开添加客户对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AssignSales()
        {
            if (SelectedCustomer == null) return;

            try
            {
                var dialog = new AssignSalesDialog();
                var viewModel = new AssignSalesDialogViewModel(_apiService, SelectedCustomer);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.AssignmentCompleted += async (sender, args) =>
                {
                    dialog.DialogResult = true;
                    await LoadCustomersAsync();
                };

                viewModel.Cancelled += (sender, args) =>
                {
                    dialog.DialogResult = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开分配销售对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AssignSupport()
        {
            if (SelectedCustomer == null) return;

            try
            {
                var dialog = new AssignSupportDialog();
                var viewModel = new AssignSupportDialogViewModel(_apiService, SelectedCustomer);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.AssignmentCompleted += async (sender, args) =>
                {
                    dialog.DialogResult = true;
                    await LoadCustomersAsync();
                };

                viewModel.Cancelled += (sender, args) =>
                {
                    dialog.DialogResult = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开分配客服对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewContacts(Customer? customer)
        {
            if (customer == null) return;

            try
            {
                var dialog = new ViewContactsDialog();
                var viewModel = new ViewContactsDialogViewModel(customer);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.CloseRequested += (sender, args) =>
                {
                    dialog.Close();
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开查看联系人对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Row-level command implementations
        private void EditCustomerRow(Customer? customer)
        {
            if (customer == null) return;

            try
            {
                var dialog = new Views.Dialogs.EditCustomerDialog();
                var viewModel = new EditCustomerDialogViewModel(_apiService, customer);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.CustomerSaved += async (sender, args) =>
                {
                    dialog.DialogResult = true;
                    await LoadCustomersAsync();
                };

                viewModel.Cancelled += (sender, args) =>
                {
                    dialog.DialogResult = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开编辑客户对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteCustomerRowAsync(Customer? customer)
        {
            if (customer == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"确定要删除客户 '{customer.Name}' 吗？此操作不可撤销。",
                    "确认删除",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // TODO: Call API to delete customer when backend is ready
                    // await _apiService.DeleteCustomerAsync(customer.Id);

                    // For now, just remove from the collection
                    Customers.Remove(customer);

                    MessageBox.Show($"客户 '{customer.Name}' 已成功删除", "删除成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除客户失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditCustomer()
        {
            if (SelectedCustomer == null) return;

            try
            {
                var dialog = new Views.Dialogs.EditCustomerDialog();
                var viewModel = new EditCustomerDialogViewModel(_apiService, SelectedCustomer);
                dialog.DataContext = viewModel;

                viewModel.CustomerSaved += async (sender, args) =>
                {
                    dialog.DialogResult = true;
                    await LoadCustomersAsync();
                };

                viewModel.Cancelled += (sender, args) =>
                {
                    dialog.DialogResult = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开编辑客户对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteCustomerAsync()
        {
            if (SelectedCustomer == null) return;

            var result = MessageBox.Show(
                $"确定要删除客户 '{SelectedCustomer.Name}' 吗？此操作不可撤销。",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _apiService.DeleteCustomerAsync(SelectedCustomer.Id);
                    await LoadCustomersAsync();
                    MessageBox.Show("客户删除成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除客户失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }
}
