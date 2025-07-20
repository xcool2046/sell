using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class SystemSettingsViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Department> _departments;
        private ObservableCollection<DepartmentGroup> _departmentGroups;
        private ObservableCollection<Role> _roles;
        private ObservableCollection<Employee> _employees;
        private Department? _selectedDepartment;
        private DepartmentGroup? _selectedDepartmentGroup;
        private Role? _selectedRole;
        private Employee? _selectedEmployee;
        private bool _isLoading;

        // Tab navigation properties
        private bool _isDepartmentTabActive = true;
        private bool _isDepartmentGroupTabActive = false;
        private bool _isEmployeeTabActive = false;
        private bool _isPermissionTabActive = false;

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

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set => SetProperty(ref _employees, value);
        }

        public Department? SelectedDepartment
        {
            get => _selectedDepartment;
            set => SetProperty(ref _selectedDepartment, value);
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

        public Employee? SelectedEmployee
        {
            get => _selectedEmployee;
            set => SetProperty(ref _selectedEmployee, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Tab navigation properties
        public bool IsDepartmentTabActive
        {
            get => _isDepartmentTabActive;
            set => SetProperty(ref _isDepartmentTabActive, value);
        }

        public bool IsDepartmentGroupTabActive
        {
            get => _isDepartmentGroupTabActive;
            set => SetProperty(ref _isDepartmentGroupTabActive, value);
        }

        public bool IsEmployeeTabActive
        {
            get => _isEmployeeTabActive;
            set => SetProperty(ref _isEmployeeTabActive, value);
        }

        public bool IsPermissionTabActive
        {
            get => _isPermissionTabActive;
            set => SetProperty(ref _isPermissionTabActive, value);
        }

        // Navigation Commands
        public ICommand ShowDepartmentManagementCommand { get; }
        public ICommand ShowDepartmentGroupCommand { get; }
        public ICommand ShowEmployeeManagementCommand { get; }
        public ICommand ShowUserPermissionCommand { get; }

        // Department Commands
        public ICommand AddDepartmentCommand { get; }
        public ICommand EditDepartmentCommand { get; }
        public ICommand DeleteDepartmentCommand { get; }

        // Department Group Commands
        public ICommand AddDepartmentGroupCommand { get; }
        public ICommand EditDepartmentGroupCommand { get; }
        public ICommand DeleteDepartmentGroupCommand { get; }

        // Employee Commands
        public ICommand AddEmployeeCommand { get; }
        public ICommand EditEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }

        // Permission Commands
        public ICommand AddPermissionCommand { get; }
        public ICommand EditPermissionCommand { get; }
        public ICommand DeletePermissionCommand { get; }

        public SystemSettingsViewModel()
        {
            _apiService = new ApiService();
            _departments = new ObservableCollection<Department>();
            _departmentGroups = new ObservableCollection<DepartmentGroup>();
            _roles = new ObservableCollection<Role>();
            _employees = new ObservableCollection<Employee>();

            // Initialize navigation commands
            ShowDepartmentManagementCommand = new RelayCommand(p => ShowDepartmentManagement());
            ShowDepartmentGroupCommand = new RelayCommand(p => ShowDepartmentGroup());
            ShowEmployeeManagementCommand = new RelayCommand(p => ShowEmployeeManagement());
            ShowUserPermissionCommand = new RelayCommand(p => ShowUserPermission());

            // Initialize department commands
            AddDepartmentCommand = new RelayCommand(p => AddDepartment());
            EditDepartmentCommand = new RelayCommand(p => EditDepartment(p as Department));
            DeleteDepartmentCommand = new AsyncRelayCommand(async p => await DeleteDepartmentAsync(p as Department));

            // Initialize department group commands
            AddDepartmentGroupCommand = new RelayCommand(p => AddDepartmentGroup());
            EditDepartmentGroupCommand = new RelayCommand(p => EditDepartmentGroup(p as DepartmentGroup));
            DeleteDepartmentGroupCommand = new AsyncRelayCommand(async p => await DeleteDepartmentGroupAsync(p as DepartmentGroup));

            // Initialize employee commands
            AddEmployeeCommand = new RelayCommand(p => AddEmployee());
            EditEmployeeCommand = new RelayCommand(p => EditEmployee(p as Employee));
            DeleteEmployeeCommand = new AsyncRelayCommand(async p => await DeleteEmployeeAsync(p as Employee));

            // Initialize permission commands
            AddPermissionCommand = new RelayCommand(p => AddPermission());
            EditPermissionCommand = new RelayCommand(p => EditPermission(p as Role));
            DeletePermissionCommand = new AsyncRelayCommand(async p => await DeletePermissionAsync(p as Role));
        }

        public override async Task LoadDataAsync()
        {
            if (IsDataLoaded) return; // Avoid loading data multiple times
            await LoadAllSystemDataAsync();
            IsDataLoaded = true;
        }

        private async Task LoadAllSystemDataAsync()
        {
            try
            {
                IsLoading = true;

                var departmentsTask = LoadDepartmentsAsync();
                var departmentGroupsTask = LoadDepartmentGroupsAsync();
                var rolesTask = LoadRolesAsync();
                var employeesTask = LoadEmployeesAsync();

                await Task.WhenAll(departmentsTask, departmentGroupsTask, rolesTask, employeesTask);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载系统设置数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region Navigation Methods

        private void ShowDepartmentManagement()
        {
            IsDepartmentTabActive = true;
            IsDepartmentGroupTabActive = false;
            IsEmployeeTabActive = false;
            IsPermissionTabActive = false;
        }

        private void ShowDepartmentGroup()
        {
            IsDepartmentTabActive = false;
            IsDepartmentGroupTabActive = true;
            IsEmployeeTabActive = false;
            IsPermissionTabActive = false;
        }

        private void ShowEmployeeManagement()
        {
            IsDepartmentTabActive = false;
            IsDepartmentGroupTabActive = false;
            IsEmployeeTabActive = true;
            IsPermissionTabActive = false;
        }

        private void ShowUserPermission()
        {
            IsDepartmentTabActive = false;
            IsDepartmentGroupTabActive = false;
            IsEmployeeTabActive = false;
            IsPermissionTabActive = true;
        }

        #endregion

        #region Data Loading Methods

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var departments = await _apiService.GetDepartmentsAsync();

                Departments.Clear();
                foreach (var department in departments)
                {
                    Departments.Add(department);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载部门数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDepartmentGroupsAsync()
        {
            try
            {
                var groups = await _apiService.GetDepartmentGroupsAsync();

                DepartmentGroups.Clear();
                foreach (var group in groups)
                {
                    DepartmentGroups.Add(group);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载部门分组数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                var roles = await _apiService.GetRolesAsync();

                Roles.Clear();
                foreach (var role in roles)
                {
                    Roles.Add(role);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载角色数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadEmployeesAsync()
        {
            try
            {
                var employees = await _apiService.GetEmployeesAsync();

                Employees.Clear();
                foreach (var employee in employees)
                {
                    Employees.Add(employee);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载员工数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Department Methods

        private void AddDepartment()
        {
            try
            {
                var dialog = new Views.Dialogs.AddDepartmentDialog();
                var viewModel = new ViewModels.Dialogs.AddDepartmentDialogViewModel(_apiService);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.DepartmentSaved += async (sender, args) =>
                {
                    dialog.DialogResult = true;
                    await LoadDepartmentsAsync();
                };

                viewModel.Cancelled += (sender, args) =>
                {
                    dialog.DialogResult = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开添加部门对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditDepartment(Department? department)
        {
            if (department == null) return;

            try
            {
                var dialog = new Views.Dialogs.EditDepartmentDialog();
                var viewModel = new ViewModels.Dialogs.EditDepartmentDialogViewModel(_apiService, department);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.DepartmentSaved += async (sender, args) =>
                {
                    dialog.DialogResult = true;
                    await LoadDepartmentsAsync();
                };

                viewModel.Cancelled += (sender, args) =>
                {
                    dialog.DialogResult = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开编辑部门对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteDepartmentAsync(Department? department)
        {
            if (department == null) return;

            var result = MessageBox.Show(
                $"确定要删除部门 '{department.Name}' 吗？此操作不可撤销。",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _apiService.DeleteDepartmentAsync(department.Id);
                    await LoadDepartmentsAsync();
                    MessageBox.Show("部门删除成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除部门失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        #endregion

        #region Department Group Methods

        private void AddDepartmentGroup()
        {
            try
            {
                var dialog = new Views.Dialogs.AddDepartmentGroupDialog();
                var viewModel = new ViewModels.Dialogs.AddDepartmentGroupDialogViewModel(_apiService);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.DepartmentGroupSaved += async (sender, args) =>
                {
                    dialog.DialogResult = true;
                    await LoadDepartmentGroupsAsync();
                };

                viewModel.Cancelled += (sender, args) =>
                {
                    dialog.DialogResult = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开添加部门分组对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditDepartmentGroup(DepartmentGroup? group)
        {
            if (group == null) return;

            try
            {
                var dialog = new Views.Dialogs.EditDepartmentGroupDialog();
                var viewModel = new ViewModels.Dialogs.EditDepartmentGroupDialogViewModel(_apiService, group);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.GroupSaved += async (sender, args) =>
                {
                    dialog.DialogResult = true;
                    await LoadDepartmentGroupsAsync();
                };

                viewModel.Cancelled += (sender, args) =>
                {
                    dialog.DialogResult = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开编辑部门分组对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteDepartmentGroupAsync(DepartmentGroup? group)
        {
            if (group == null) return;

            var result = MessageBox.Show(
                $"确定要删除分组 '{group.Name}' 吗？此操作不可撤销。",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _apiService.DeleteDepartmentGroupAsync(group.Id);
                    await LoadDepartmentGroupsAsync();
                    MessageBox.Show("分组删除成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除分组失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        #endregion

        #region Employee Methods

        private void AddEmployee()
        {
            try
            {
                var dialog = new Views.Dialogs.AddEmployeeDialog();
                var viewModel = new ViewModels.Dialogs.AddEmployeeDialogViewModel(_apiService);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.EmployeeSaved += async (sender, args) =>
                {
                    dialog.DialogResult = true;
                    await LoadEmployeesAsync();
                };

                viewModel.Cancelled += (sender, args) =>
                {
                    dialog.DialogResult = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开添加员工对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditEmployee(Employee? employee)
        {
            if (employee == null) return;

            try
            {
                var dialog = new Views.Dialogs.EditEmployeeDialog();
                var viewModel = new ViewModels.Dialogs.EditEmployeeDialogViewModel(_apiService, employee);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.EmployeeSaved += async (sender, args) =>
                {
                    dialog.DialogResult = true;
                    await LoadEmployeesAsync();
                };

                viewModel.Cancelled += (sender, args) =>
                {
                    dialog.DialogResult = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开编辑员工对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteEmployeeAsync(Employee? employee)
        {
            if (employee == null) return;

            var result = MessageBox.Show(
                $"确定要删除员工 '{employee.Name}' 吗？此操作不可撤销。",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _apiService.DeleteEmployeeAsync(employee.Id);
                    await LoadEmployeesAsync();
                    MessageBox.Show("员工删除成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除员工失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        #endregion

        #region Permission Methods

        private void AddPermission()
        {
            try
            {
                var dialog = new Views.Dialogs.AddPermissionDialog();
                var viewModel = new ViewModels.Dialogs.AddPermissionDialogViewModel(_apiService);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.PermissionSaved += async (sender, args) =>
                {
                    dialog.DialogResult = true;
                    await LoadRolesAsync();
                };

                viewModel.Cancelled += (sender, args) =>
                {
                    dialog.DialogResult = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开添加权限对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditPermission(Role? role)
        {
            if (role == null) return;

            try
            {
                var dialog = new Views.Dialogs.EditPermissionDialog();
                var viewModel = new ViewModels.Dialogs.EditPermissionDialogViewModel(_apiService, role);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.PermissionSaved += async (sender, args) =>
                {
                    dialog.DialogResult = true;
                    await LoadRolesAsync();
                };

                viewModel.Cancelled += (sender, args) =>
                {
                    dialog.DialogResult = false;
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开编辑权限对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeletePermissionAsync(Role? role)
        {
            if (role == null) return;

            var result = MessageBox.Show(
                $"确定要删除角色 '{role.Name}' 的权限设置吗？此操作不可撤销。",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _apiService.DeleteRoleAsync(role.Id);
                    await LoadRolesAsync();
                    MessageBox.Show("权限删除成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除权限失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        #endregion
    }
}
