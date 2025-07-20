using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using Sellsys.WpfClient.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class SalesManagementViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Employee> _employees;
        private Customer? _selectedCustomer;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private bool _isAllSelected;

        // Filter properties
        private ObservableCollection<string> _industryTypes;
        private ObservableCollection<string> _provinces;
        private ObservableCollection<string> _cities;
        private ObservableCollection<string> _contactStatuses;
        private ObservableCollection<string> _responsiblePersons;
        private string _selectedIndustryType = "全部";
        private string _selectedProvince = "全部";
        private string _selectedCity = "全部";
        private string _selectedContactStatus = "全部";
        private string _selectedResponsiblePerson = "全部";

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

        public bool IsAllSelected
        {
            get => _isAllSelected;
            set => SetProperty(ref _isAllSelected, value);
        }

        // Filter collections
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

        // Filter selections
        public string SelectedIndustryType
        {
            get => _selectedIndustryType;
            set => SetProperty(ref _selectedIndustryType, value);
        }

        public string SelectedProvince
        {
            get => _selectedProvince;
            set => SetProperty(ref _selectedProvince, value);
        }

        public string SelectedCity
        {
            get => _selectedCity;
            set => SetProperty(ref _selectedCity, value);
        }

        public string SelectedContactStatus
        {
            get => _selectedContactStatus;
            set => SetProperty(ref _selectedContactStatus, value);
        }

        public string SelectedResponsiblePerson
        {
            get => _selectedResponsiblePerson;
            set => SetProperty(ref _selectedResponsiblePerson, value);
        }

        // Commands
        public ICommand LoadDataCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand ViewContactsCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand ViewContactRecordsCommand { get; }
        public ICommand ViewOrdersCommand { get; }
        public ICommand ViewOrderRecordsCommand { get; }

        public SalesManagementViewModel()
        {
            _apiService = new ApiService();
            _customers = new ObservableCollection<Customer>();
            _employees = new ObservableCollection<Employee>();
            _industryTypes = new ObservableCollection<string>();
            _provinces = new ObservableCollection<string>();
            _cities = new ObservableCollection<string>();
            _contactStatuses = new ObservableCollection<string>();
            _responsiblePersons = new ObservableCollection<string>();

            // Initialize commands
            LoadDataCommand = new AsyncRelayCommand(async p => await LoadCustomersAsync());
            SearchCommand = new AsyncRelayCommand(async p => await SearchCustomersAsync());
            ClearFiltersCommand = new RelayCommand(p => ClearFilters());
            ViewContactsCommand = new RelayCommand(p => ViewContacts(p as Customer));
            EditCustomerCommand = new RelayCommand(p => EditCustomer(p as Customer));
            ViewDetailsCommand = new RelayCommand(p => ViewDetails(p as Customer));
            SelectAllCommand = new RelayCommand(p => ToggleSelectAll());
            ViewContactRecordsCommand = new RelayCommand(p => ViewContactRecords(p as Customer));
            ViewOrdersCommand = new RelayCommand(p => ViewOrders(p as Customer));
            ViewOrderRecordsCommand = new RelayCommand(p => ViewOrderRecords(p as Customer));

            // Initialize filter data
            InitializeFilters();

            // Note: Data loading is now triggered manually or when view becomes active
            // This prevents API calls during application startup
        }

        public override async Task LoadDataAsync()
        {
            if (IsDataLoaded) return; // Avoid loading data multiple times
            await LoadCustomersAsync();
            IsDataLoaded = true;
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                IsLoading = true;

                // Load all required data
                var customersTask = _apiService.GetCustomersAsync();
                var employeesTask = _apiService.GetEmployeesAsync();

                await Task.WhenAll(customersTask, employeesTask);

                // Update collections - these should already be on UI thread since LoadDataAsync is called from UI
                Customers.Clear();
                foreach (var customer in customersTask.Result)
                {
                    // Add next contact date from latest follow-up log
                    await EnrichCustomerWithFollowUpData(customer);
                    Customers.Add(customer);
                }

                Employees.Clear();
                foreach (var employee in employeesTask.Result)
                {
                    Employees.Add(employee);
                }

                // Update filter options based on loaded data
                UpdateFilterOptions();
            }
            catch (Exception ex)
            {
                // Use the error handling service
                ErrorHandlingService.HandleApiError(ex, "loading customer data");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EnrichCustomerWithFollowUpData(Customer customer)
        {
            try
            {
                // Get latest follow-up log for this customer to get next contact date
                var followUpLogs = await _apiService.GetSalesFollowUpLogsAsync();
                var latestLog = followUpLogs
                    .Where(log => log.CustomerId == customer.Id)
                    .OrderByDescending(log => log.CreatedAt)
                    .FirstOrDefault();

                if (latestLog != null)
                {
                    customer.NextContactDate = latestLog.NextFollowUpDate;
                    customer.CustomerIntention = latestLog.CustomerIntention ?? "待分配";
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the whole operation
                System.Diagnostics.Debug.WriteLine($"Error enriching customer {customer.Id}: {ex.Message}");
            }
        }

        private async Task SearchCustomersAsync()
        {
            try
            {
                IsLoading = true;
                var allCustomers = await _apiService.GetCustomersAsync();

                var filteredCustomers = allCustomers.AsEnumerable();

                // Apply text search
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filteredCustomers = filteredCustomers.Where(customer =>
                        customer.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(customer.Remarks) && customer.Remarks.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                        customer.Contacts.Any(c => c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    );
                }

                // Apply industry type filter
                if (SelectedIndustryType != "全部")
                {
                    filteredCustomers = filteredCustomers.Where(customer =>
                        !string.IsNullOrEmpty(customer.IndustryTypes) &&
                        customer.IndustryTypes.Contains(SelectedIndustryType, StringComparison.OrdinalIgnoreCase)
                    );
                }

                // Apply province filter
                if (SelectedProvince != "全部")
                {
                    filteredCustomers = filteredCustomers.Where(customer => customer.Province == SelectedProvince);
                }

                // Apply city filter
                if (SelectedCity != "全部")
                {
                    filteredCustomers = filteredCustomers.Where(customer => customer.City == SelectedCity);
                }

                // Apply contact status filter
                if (SelectedContactStatus != "全部")
                {
                    filteredCustomers = filteredCustomers.Where(customer => customer.Status == SelectedContactStatus);
                }

                // Apply responsible person filter
                if (SelectedResponsiblePerson != "全部")
                {
                    filteredCustomers = filteredCustomers.Where(customer =>
                        customer.SalesPersonName == SelectedResponsiblePerson ||
                        customer.SupportPersonName == SelectedResponsiblePerson
                    );
                }

                Customers.Clear();
                foreach (var customer in filteredCustomers.OrderByDescending(c => c.CreatedAt))
                {
                    // Enrich with follow-up data
                    await EnrichCustomerWithFollowUpData(customer);
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

        private void InitializeFilters()
        {
            // Initialize industry types
            IndustryTypes.Clear();
            IndustryTypes.Add("全部");
            IndustryTypes.Add("教育");
            IndustryTypes.Add("医疗");
            IndustryTypes.Add("金融");
            IndustryTypes.Add("制造业");
            IndustryTypes.Add("服务业");
            IndustryTypes.Add("政府");
            IndustryTypes.Add("其他");

            // Initialize provinces
            Provinces.Clear();
            Provinces.Add("全部");
            Provinces.Add("四川");
            Provinces.Add("广西");
            Provinces.Add("广东");
            Provinces.Add("北京");
            Provinces.Add("上海");
            Provinces.Add("重庆");
            Provinces.Add("天津");

            // Initialize cities
            Cities.Clear();
            Cities.Add("全部");
            Cities.Add("成都");
            Cities.Add("桂林");
            Cities.Add("广州");
            Cities.Add("深圳");

            // Initialize contact statuses
            ContactStatuses.Clear();
            ContactStatuses.Add("全部");
            ContactStatuses.Add("待跟进");
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

        private void UpdateFilterOptions()
        {
            // Update provinces based on loaded customers
            var provinces = Customers.Where(c => !string.IsNullOrEmpty(c.Province))
                                   .Select(c => c.Province!)
                                   .Distinct()
                                   .OrderBy(p => p)
                                   .ToList();

            Provinces.Clear();
            Provinces.Add("全部");
            foreach (var province in provinces)
            {
                Provinces.Add(province);
            }

            // Update cities based on loaded customers
            var cities = Customers.Where(c => !string.IsNullOrEmpty(c.City))
                                .Select(c => c.City!)
                                .Distinct()
                                .OrderBy(c => c)
                                .ToList();

            Cities.Clear();
            Cities.Add("全部");
            foreach (var city in cities)
            {
                Cities.Add(city);
            }

            // Update responsible persons based on loaded employees
            ResponsiblePersons.Clear();
            ResponsiblePersons.Add("全部");
            foreach (var employee in Employees.OrderBy(e => e.Name))
            {
                ResponsiblePersons.Add(employee.Name);
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

        private void EditCustomer(Customer? customer)
        {
            if (customer == null) return;

            // TODO: Show EditCustomerDialog
            MessageBox.Show($"编辑客户: {customer.Name}", "编辑客户", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewDetails(Customer? customer)
        {
            if (customer == null) return;

            // TODO: Show CustomerDetailsDialog
            MessageBox.Show($"查看客户详情: {customer.Name}", "客户详情", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearFilters()
        {
            SelectedIndustryType = "全部";
            SelectedProvince = "全部";
            SelectedCity = "全部";
            SelectedContactStatus = "全部";
            SelectedResponsiblePerson = "全部";
            SearchText = string.Empty;
        }

        private void ToggleSelectAll()
        {
            // 检查当前是否所有项都已选中
            bool allSelected = Customers.All(c => c.IsSelected);

            // 如果全部选中，则取消全选；否则全选
            IsAllSelected = !allSelected;

            foreach (var customer in Customers)
            {
                customer.IsSelected = IsAllSelected;
            }
        }

        private void ViewContactRecords(Customer? customer)
        {
            if (customer == null) return;

            try
            {
                var dialog = new ContactRecordsDialog();
                var viewModel = new ContactRecordsDialogViewModel(customer);
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
                MessageBox.Show($"打开联系记录对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewOrders(Customer? customer)
        {
            if (customer == null) return;

            // TODO: Show OrdersDialog
            MessageBox.Show($"查看订单: {customer.Name}", "订单记录", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewOrderRecords(Customer? customer)
        {
            if (customer == null) return;

            try
            {
                var dialog = new OrderRecordsDialog();
                var viewModel = new OrderRecordsDialogViewModel(customer);
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
                MessageBox.Show($"打开订单记录对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
