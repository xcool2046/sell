using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class AssignSalesDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Customer _customer;

        private string _departmentName = "销售部";
        private ObservableCollection<DepartmentGroup> _groups;
        private ObservableCollection<Employee> _salesPersons;
        private DepartmentGroup? _selectedGroup;
        private Employee? _selectedSalesPerson;
        private bool _isLoading = false;

        public string DepartmentName
        {
            get => _departmentName;
            set => SetProperty(ref _departmentName, value);
        }

        public ObservableCollection<DepartmentGroup> Groups
        {
            get => _groups;
            set => SetProperty(ref _groups, value);
        }

        public ObservableCollection<Employee> SalesPersons
        {
            get => _salesPersons;
            set => SetProperty(ref _salesPersons, value);
        }

        public DepartmentGroup? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                SetProperty(ref _selectedGroup, value);
                _ = LoadSalesPersonsForGroupAsync();
            }
        }

        public Employee? SelectedSalesPerson
        {
            get => _selectedSalesPerson;
            set => SetProperty(ref _selectedSalesPerson, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Commands
        public ICommand AssignCommand { get; }
        public ICommand CancelCommand { get; }

        // Events
        public event EventHandler? AssignmentCompleted;
        public event EventHandler? Cancelled;

        public AssignSalesDialogViewModel(ApiService apiService, Customer customer)
        {
            _apiService = apiService;
            _customer = customer;

            // Initialize collections
            _groups = new ObservableCollection<DepartmentGroup>();
            _salesPersons = new ObservableCollection<Employee>();

            // Initialize commands
            AssignCommand = new AsyncRelayCommand(async p => await AssignSalesAsync());
            CancelCommand = new RelayCommand(p => Cancel());

            // Initialize data
            _ = InitializeDataAsync();
        }

        private async Task InitializeDataAsync()
        {
            try
            {
                IsLoading = true;

                // Get all departments to find sales department ID
                var departments = await _apiService.GetDepartmentsAsync();
                var salesDepartment = departments.FirstOrDefault(d => d.Name == "销售部");

                if (salesDepartment != null)
                {
                    // Load groups for sales department
                    var groups = await _apiService.GetDepartmentGroupsByDepartmentIdAsync(salesDepartment.Id);
                    Groups.Clear();
                    foreach (var group in groups)
                    {
                        Groups.Add(group);
                    }

                    // Set default selection
                    SelectedGroup = Groups.FirstOrDefault();
                }
                else
                {
                    MessageBox.Show("未找到销售部门，请先在系统设置中创建销售部门", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
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

        private async Task LoadSalesPersonsForGroupAsync()
        {
            try
            {
                SalesPersons.Clear();

                if (SelectedGroup == null) return;

                // Load employees from sales department
                var employees = await _apiService.GetEmployeesByDepartmentAsync("销售部");

                // Filter employees by selected group if needed
                var filteredEmployees = employees.Where(e => e.GroupId == SelectedGroup.Id).ToList();

                foreach (var employee in filteredEmployees)
                {
                    SalesPersons.Add(employee);
                }

                SelectedSalesPerson = SalesPersons.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载销售人员失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AssignSalesAsync()
        {
            if (IsLoading) return; // Prevent multiple simultaneous assignments

            try
            {
                IsLoading = true;
                if (SelectedSalesPerson == null)
                {
                    MessageBox.Show("请选择销售人员", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Call API to assign sales person to customer
                await _apiService.AssignSalesPersonAsync(_customer.Id, SelectedSalesPerson.Id);

                // Update customer's sales person
                _customer.SalesPersonId = SelectedSalesPerson.Id;
                _customer.SalesPersonName = SelectedSalesPerson.Name;

                // Publish assignment event for other modules
                EventBus.Instance.Publish(new CustomerAssignedEvent
                {
                    CustomerId = _customer.Id,
                    CustomerName = _customer.Name,
                    SalesPersonId = SelectedSalesPerson.Id,
                    SalesPersonName = SelectedSalesPerson.Name,
                    AssignedAt = DateTime.Now,
                    AssignmentType = "Sales"
                });

                MessageBox.Show($"已成功将客户 '{_customer.Name}' 分配给销售 '{SelectedSalesPerson.Name}'",
                    "分配成功", MessageBoxButton.OK, MessageBoxImage.Information);

                AssignmentCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"分配销售失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Cancel()
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }
}
