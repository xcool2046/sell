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
    public class AddEmployeeDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly EventBus _eventBus;
        private ObservableCollection<Department> _departments;
        private ObservableCollection<DepartmentGroup> _departmentGroups;
        private ObservableCollection<Role> _roles;
        private Department? _selectedDepartment;
        private DepartmentGroup? _selectedDepartmentGroup;
        private Role? _selectedRole;
        private string _employeeName = string.Empty;
        private string _phoneNumber = string.Empty;
        private string _loginUsername = string.Empty;
        private string _initialPassword = "abc12345";
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
                    _ = LoadDepartmentGroupsForDepartmentAsync();
                }
            }
        }

        public DepartmentGroup? SelectedDepartmentGroup
        {
            get => _selectedDepartmentGroup;
            set => SetProperty(ref _selectedDepartmentGroup, value);
        }

        public Role? SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
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

        public string LoginUsername
        {
            get => _loginUsername;
            set => SetProperty(ref _loginUsername, value);
        }

        public string InitialPassword
        {
            get => _initialPassword;
            set => SetProperty(ref _initialPassword, value);
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

        public AddEmployeeDialogViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _eventBus = EventBus.Instance;
            _departments = new ObservableCollection<Department>();
            _departmentGroups = new ObservableCollection<DepartmentGroup>();
            _roles = new ObservableCollection<Role>();

            SaveCommand = new AsyncRelayCommand(async p => await SaveEmployeeAsync(), p => CanSave());
            CancelCommand = new RelayCommand(p => Cancel());

            // 订阅事件
            _eventBus.Subscribe<DepartmentUpdatedEvent>(OnDepartmentUpdated);
            _eventBus.Subscribe<DepartmentDeletedEvent>(OnDepartmentDeleted);
            _eventBus.Subscribe<DepartmentGroupUpdatedEvent>(OnDepartmentGroupUpdated);

            // Load initial data when dialog opens
            _ = LoadInitialDataAsync();
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(EmployeeName) &&
                   !string.IsNullOrWhiteSpace(LoginUsername) &&
                   SelectedRole != null &&
                   !IsSaving &&
                   !IsLoading;
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                IsLoading = true;

                // Load departments and roles in parallel
                var departmentsTask = _apiService.GetDepartmentsAsync();
                var rolesTask = _apiService.GetRolesAsync();

                await Task.WhenAll(departmentsTask, rolesTask);

                // Update departments
                Departments.Clear();
                foreach (var department in await departmentsTask)
                {
                    Departments.Add(department);
                }

                // Update roles
                Roles.Clear();
                foreach (var role in await rolesTask)
                {
                    Roles.Add(role);
                }

                // Auto-select first department if only one exists
                if (Departments.Count == 1)
                {
                    SelectedDepartment = Departments.First();
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

        private async Task LoadDepartmentGroupsForDepartmentAsync()
        {
            if (SelectedDepartment == null)
            {
                DepartmentGroups.Clear();
                SelectedDepartmentGroup = null;
                return;
            }

            try
            {
                var groups = await _apiService.GetDepartmentGroupsByDepartmentIdAsync(SelectedDepartment.Id);
                
                DepartmentGroups.Clear();
                foreach (var group in groups)
                {
                    DepartmentGroups.Add(group);
                }

                // Auto-select first group if only one exists
                if (DepartmentGroups.Count == 1)
                {
                    SelectedDepartmentGroup = DepartmentGroups.First();
                }
                else
                {
                    SelectedDepartmentGroup = null;
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

                // 验证输入
                if (string.IsNullOrWhiteSpace(EmployeeName))
                {
                    MessageBox.Show("请输入员工姓名", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(LoginUsername))
                {
                    MessageBox.Show("请输入登录账号", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedRole == null)
                {
                    MessageBox.Show("请选择岗位职务", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(InitialPassword))
                {
                    MessageBox.Show("请输入初始密码", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (InitialPassword.Length < 6)
                {
                    MessageBox.Show("密码长度至少6位", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 检查用户名是否已存在
                try
                {
                    var existingEmployees = await _apiService.GetEmployeesAsync();
                    if (existingEmployees.Any(e => e.LoginUsername.Equals(LoginUsername.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show($"登录账号 '{LoginUsername.Trim()}' 已存在，请使用其他账号", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"检查用户名失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 创建员工DTO
                var employeeDto = new EmployeeUpsertDto
                {
                    Name = EmployeeName.Trim(),
                    LoginUsername = LoginUsername.Trim(),
                    Phone = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
                    GroupId = SelectedDepartmentGroup?.Id,
                    RoleId = SelectedRole.Id,
                    Password = InitialPassword.Trim()
                };

                // 调用API创建员工
                await _apiService.CreateEmployeeAsync(employeeDto);

                // 通知保存成功
                EmployeeSaved?.Invoke(this, EventArgs.Empty);
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加员工失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private async void OnDepartmentUpdated(DepartmentUpdatedEvent eventData)
        {
            try
            {
                await LoadInitialDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"处理部门更新事件失败: {ex.Message}");
            }
        }

        private async void OnDepartmentDeleted(DepartmentDeletedEvent eventData)
        {
            try
            {
                await LoadInitialDataAsync();

                // 如果当前选择的部门被删除了，清除选择
                if (SelectedDepartment?.Id == eventData.DepartmentId)
                {
                    SelectedDepartment = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"处理部门删除事件失败: {ex.Message}");
            }
        }

        private async void OnDepartmentGroupUpdated(DepartmentGroupUpdatedEvent eventData)
        {
            try
            {
                // 如果当前选择的部门有分组更新，重新加载分组
                if (SelectedDepartment?.Id == eventData.DepartmentId)
                {
                    await LoadDepartmentGroupsForDepartmentAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"处理部门分组更新事件失败: {ex.Message}");
            }
        }
    }
}
