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

        private ObservableCollection<Department> _departments;
        private ObservableCollection<DepartmentGroup> _groups;
        private ObservableCollection<Employee> _salesPersons;
        private Department? _selectedDepartment;
        private DepartmentGroup? _selectedGroup;
        private Employee? _selectedSalesPerson;
        private bool _isLoading = false;

        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        public Department? SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                if (SetProperty(ref _selectedDepartment, value))
                {
                    _ = OnSelectedDepartmentChangedAsync();
                }
            }
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
            _departments = new ObservableCollection<Department>();
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

                // Load all departments
                var departments = await _apiService.GetDepartmentsAsync();
                Departments.Clear();
                foreach (var department in departments)
                {
                    Departments.Add(department);
                }

                // Try to select "销售部" as default, or first department if not found
                var salesDepartment = departments.FirstOrDefault(d => d.Name == "销售部");
                SelectedDepartment = salesDepartment ?? departments.FirstOrDefault();

                if (Departments.Count == 0)
                {
                    MessageBox.Show("未找到任何部门，请先在系统设置中创建部门", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private async Task OnSelectedDepartmentChangedAsync()
        {
            if (SelectedDepartment == null)
            {
                Groups.Clear();
                SalesPersons.Clear();
                return;
            }

            try
            {
                IsLoading = true;

                // Load groups for selected department
                var groups = await _apiService.GetDepartmentGroupsByDepartmentIdAsync(SelectedDepartment.Id);
                Groups.Clear();
                foreach (var group in groups)
                {
                    Groups.Add(group);
                }

                // Set default selection
                SelectedGroup = Groups.FirstOrDefault();

                // Clear sales persons as they depend on group selection
                SalesPersons.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载部门分组失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
