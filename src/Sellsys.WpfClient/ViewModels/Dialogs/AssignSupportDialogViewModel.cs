using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class AssignSupportDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Customer _customer;

        private ObservableCollection<Department> _departments;
        private ObservableCollection<DepartmentGroup> _groups;
        private ObservableCollection<Employee> _supportPersons;
        private Department? _selectedDepartment;
        private DepartmentGroup? _selectedGroup;
        private Employee? _selectedSupportPerson;
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

        public ObservableCollection<Employee> SupportPersons
        {
            get => _supportPersons;
            set => SetProperty(ref _supportPersons, value);
        }

        public DepartmentGroup? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                SetProperty(ref _selectedGroup, value);
                _ = LoadSupportPersonsForGroupAsync();
            }
        }

        public Employee? SelectedSupportPerson
        {
            get => _selectedSupportPerson;
            set => SetProperty(ref _selectedSupportPerson, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Commands
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Events
        public event EventHandler? AssignmentCompleted;
        public event EventHandler? Cancelled;

        public AssignSupportDialogViewModel(ApiService apiService, Customer customer)
        {
            _apiService = apiService;
            _customer = customer;

            // Initialize collections
            _departments = new ObservableCollection<Department>();
            _groups = new ObservableCollection<DepartmentGroup>();
            _supportPersons = new ObservableCollection<Employee>();

            // Initialize commands
            SaveCommand = new AsyncRelayCommand(async p => await AssignSupportAsync());
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

                // Try to select "客服部" as default, or first department if not found
                var supportDepartment = departments.FirstOrDefault(d => d.Name == "客服部");
                SelectedDepartment = supportDepartment ?? departments.FirstOrDefault();

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
                SupportPersons.Clear();
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

                // Clear support persons as they depend on group selection
                SupportPersons.Clear();
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

        private async Task LoadSupportPersonsForGroupAsync()
        {
            try
            {
                SupportPersons.Clear();

                if (SelectedGroup == null) return;

                // Load employees from support department
                var employees = await _apiService.GetEmployeesByDepartmentAsync("客服部");

                // Filter employees by selected group if needed
                var filteredEmployees = employees.Where(e => e.GroupId == SelectedGroup.Id).ToList();

                foreach (var employee in filteredEmployees)
                {
                    SupportPersons.Add(employee);
                }

                SelectedSupportPerson = SupportPersons.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载客服人员失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AssignSupportAsync()
        {
            if (IsLoading) return; // Prevent multiple simultaneous assignments

            try
            {
                IsLoading = true;
                if (SelectedSupportPerson == null)
                {
                    MessageBox.Show("请选择客服人员", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Call API to assign support person to customer
                await _apiService.AssignSupportPersonAsync(_customer.Id, SelectedSupportPerson.Id);

                // Update customer's support person
                _customer.SupportPersonId = SelectedSupportPerson.Id;
                _customer.SupportPersonName = SelectedSupportPerson.Name;

                // Publish assignment event for other modules
                EventBus.Instance.Publish(new CustomerAssignedEvent
                {
                    CustomerId = _customer.Id,
                    CustomerName = _customer.Name,
                    SupportPersonId = SelectedSupportPerson.Id,
                    SupportPersonName = SelectedSupportPerson.Name,
                    AssignedAt = DateTime.Now,
                    AssignmentType = "Support"
                });

                AssignmentCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"分配客服失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
