using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Sellsys.WpfClient.ViewModels.Dialogs
{
    public class EditEmployeeDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Employee _originalEmployee;
        private ObservableCollection<Department> _departments;
        private ObservableCollection<DepartmentGroup> _departmentGroups;
        private ObservableCollection<string> _jobPositions;
        private ObservableCollection<Role> _roles;
        private Department? _selectedDepartment;
        private DepartmentGroup? _selectedDepartmentGroup;
        private string? _selectedJobPosition;
        private string _employeeName = string.Empty;
        private string _phoneNumber = string.Empty;
        private string _loginAccount = string.Empty;
        private string _password = string.Empty;
        private bool _isSaving = false;
        private bool _isLoading = false;

        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        public ObservableCollection<DepartmentGroup> DepartmentGroups
        {
            get => _departmentGroups;
            set => SetProperty(ref _departmentGroups, value);
        }

        public ObservableCollection<string> JobPositions
        {
            get => _jobPositions;
            set => SetProperty(ref _jobPositions, value);
        }

        public ObservableCollection<Role> Roles
        {
            get => _roles;
            set => SetProperty(ref _roles, value);
        }

        public Department? SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                if (SetProperty(ref _selectedDepartment, value))
                {
                    _ = LoadDepartmentGroupsAsync();
                }
            }
        }

        public DepartmentGroup? SelectedDepartmentGroup
        {
            get => _selectedDepartmentGroup;
            set => SetProperty(ref _selectedDepartmentGroup, value);
        }

        public string? SelectedJobPosition
        {
            get => _selectedJobPosition;
            set => SetProperty(ref _selectedJobPosition, value);
        }

        public string EmployeeName
        {
            get => _employeeName;
            set => SetProperty(ref _employeeName, value);
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        public string LoginAccount
        {
            get => _loginAccount;
            set => SetProperty(ref _loginAccount, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Events
        public event EventHandler? RequestClose;
        public event EventHandler? EmployeeSaved;
        public event EventHandler? Cancelled;

        public EditEmployeeDialogViewModel(ApiService apiService, Employee employee)
        {
            _apiService = apiService;
            _originalEmployee = employee;
            _departments = new ObservableCollection<Department>();
            _departmentGroups = new ObservableCollection<DepartmentGroup>();
            _jobPositions = new ObservableCollection<string>();
            _roles = new ObservableCollection<Role>();

            // Initialize with current values
            EmployeeName = employee.Name;
            PhoneNumber = employee.Phone ?? string.Empty;
            LoginAccount = employee.LoginUsername;
            SelectedJobPosition = employee.RoleName;

            SaveCommand = new AsyncRelayCommand(async p => await SaveEmployeeAsync(), p => CanSave());
            CancelCommand = new RelayCommand(p => Cancel());

            // Load data when dialog opens
            _ = LoadEmployeeDataAsync();
        }

        private bool CanSave()
        {
            return SelectedDepartmentGroup != null &&
                   !string.IsNullOrWhiteSpace(EmployeeName) &&
                   !string.IsNullOrWhiteSpace(SelectedJobPosition) &&
                   !string.IsNullOrWhiteSpace(LoginAccount) &&
                   !IsSaving &&
                   !IsLoading &&
                   HasChanges();
        }

        private bool HasChanges()
        {
            return EmployeeName.Trim() != _originalEmployee.Name ||
                   PhoneNumber.Trim() != (_originalEmployee.Phone ?? string.Empty) ||
                   LoginAccount.Trim() != _originalEmployee.LoginUsername ||
                   SelectedJobPosition != _originalEmployee.RoleName ||
                   SelectedDepartmentGroup?.Id != _originalEmployee.GroupId;
        }

        private async Task LoadEmployeeDataAsync()
        {
            try
            {
                IsLoading = true;

                var departmentsTask = _apiService.GetDepartmentsAsync();
                var rolesTask = _apiService.GetRolesAsync();

                await Task.WhenAll(departmentsTask, rolesTask);

                // Load departments
                Departments.Clear();
                foreach (var department in departmentsTask.Result)
                {
                    Departments.Add(department);
                }

                // Load roles
                Roles.Clear();
                JobPositions.Clear();
                foreach (var role in rolesTask.Result)
                {
                    Roles.Add(role);
                    JobPositions.Add(role.Name);
                }

                // Find and set the current department based on employee's group
                if (_originalEmployee.GroupId.HasValue)
                {
                    var allGroups = await _apiService.GetDepartmentGroupsAsync();
                    var currentGroup = allGroups.FirstOrDefault(g => g.Id == _originalEmployee.GroupId.Value);
                    if (currentGroup != null)
                    {
                        SelectedDepartment = Departments.FirstOrDefault(d => d.Id == currentGroup.DepartmentId);
                        await LoadDepartmentGroupsAsync();
                        SelectedDepartmentGroup = DepartmentGroups.FirstOrDefault(g => g.Id == _originalEmployee.GroupId.Value);
                    }
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

        private async Task LoadDepartmentGroupsAsync()
        {
            if (SelectedDepartment == null) return;

            try
            {
                var groups = await _apiService.GetDepartmentGroupsByDepartmentIdAsync(SelectedDepartment.Id);
                
                DepartmentGroups.Clear();
                foreach (var group in groups)
                {
                    DepartmentGroups.Add(group);
                }

                // Auto-select if only one group
                if (DepartmentGroups.Count == 1)
                {
                    SelectedDepartmentGroup = DepartmentGroups.First();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载部门分组失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveEmployeeAsync()
        {
            try
            {
                IsSaving = true;

                // Validate input
                if (SelectedDepartmentGroup == null)
                {
                    MessageBox.Show("请选择部门分组", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(EmployeeName))
                {
                    MessageBox.Show("请输入员工姓名", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(SelectedJobPosition))
                {
                    MessageBox.Show("请选择岗位职务", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(LoginAccount))
                {
                    MessageBox.Show("请输入登录账号", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!HasChanges())
                {
                    MessageBox.Show("员工信息未发生变化", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Find the role ID for the selected job position
                var selectedRole = Roles.FirstOrDefault(r => r.Name == SelectedJobPosition);
                if (selectedRole == null)
                {
                    MessageBox.Show("找不到对应的角色信息", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create update DTO
                var employeeDto = new EmployeeUpsertDto
                {
                    Name = EmployeeName.Trim(),
                    LoginUsername = LoginAccount.Trim(),
                    Phone = PhoneNumber.Trim(),
                    BranchAccount = string.Empty, // Keep existing or empty
                    GroupId = SelectedDepartmentGroup.Id,
                    RoleId = selectedRole.Id,
                    Password = string.IsNullOrWhiteSpace(Password) ? null : Password.Trim()
                };

                // Call API to update employee
                await _apiService.UpdateEmployeeAsync(_originalEmployee.Id, employeeDto);

                // Notify success
                EmployeeSaved?.Invoke(this, EventArgs.Empty);
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新员工信息失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private void Cancel()
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
