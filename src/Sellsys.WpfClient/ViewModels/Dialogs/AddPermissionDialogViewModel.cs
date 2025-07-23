using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Constants;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Sellsys.WpfClient.ViewModels.Dialogs
{
    public class AddPermissionDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Department> _departments;
        private ObservableCollection<Role> _roles;
        private Department? _selectedDepartment;
        private Role? _selectedRole;
        private bool _customerManagementPermission = false;
        private bool _productManagementPermission = false;
        private bool _orderManagementPermission = false;
        private bool _salesFollowUpPermission = false;
        private bool _afterSalesServicePermission = false;
        private bool _financeManagementPermission = false;
        private bool _systemSettingsPermission = false;
        private bool _isSaving = false;
        private bool _isLoading = false;

        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        public ObservableCollection<Role> Roles
        {
            get => _roles;
            set => SetProperty(ref _roles, value);
        }

        public Department? SelectedDepartment
        {
            get => _selectedDepartment;
            set => SetProperty(ref _selectedDepartment, value);
        }

        public Role? SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (SetProperty(ref _selectedRole, value))
                {
                    SetDefaultPermissionsForRole();
                }
            }
        }

        public bool CustomerManagementPermission
        {
            get => _customerManagementPermission;
            set => SetProperty(ref _customerManagementPermission, value);
        }

        public bool ProductManagementPermission
        {
            get => _productManagementPermission;
            set => SetProperty(ref _productManagementPermission, value);
        }

        public bool OrderManagementPermission
        {
            get => _orderManagementPermission;
            set => SetProperty(ref _orderManagementPermission, value);
        }

        public bool SalesFollowUpPermission
        {
            get => _salesFollowUpPermission;
            set => SetProperty(ref _salesFollowUpPermission, value);
        }

        public bool AfterSalesServicePermission
        {
            get => _afterSalesServicePermission;
            set => SetProperty(ref _afterSalesServicePermission, value);
        }

        public bool FinanceManagementPermission
        {
            get => _financeManagementPermission;
            set => SetProperty(ref _financeManagementPermission, value);
        }

        public bool SystemSettingsPermission
        {
            get => _systemSettingsPermission;
            set => SetProperty(ref _systemSettingsPermission, value);
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
        public event EventHandler? PermissionSaved;
        public event EventHandler? Cancelled;

        public AddPermissionDialogViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _departments = new ObservableCollection<Department>();
            _roles = new ObservableCollection<Role>();

            SaveCommand = new AsyncRelayCommand(async p => await SavePermissionAsync(), p => CanSave());
            CancelCommand = new RelayCommand(p => Cancel());

            // Load departments and roles when dialog opens
            _ = LoadInitialDataAsync();
        }

        private bool CanSave()
        {
            return SelectedDepartment != null &&
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

        private void SetDefaultPermissionsForRole()
        {
            if (SelectedRole == null)
            {
                // 清除所有权限
                CustomerManagementPermission = false;
                ProductManagementPermission = false;
                OrderManagementPermission = false;
                SalesFollowUpPermission = false;
                AfterSalesServicePermission = false;
                FinanceManagementPermission = false;
                SystemSettingsPermission = false;
                return;
            }

            // 根据角色的权限配置设置复选框
            var modules = string.IsNullOrEmpty(SelectedRole.AccessibleModules)
                ? new List<string>()
                : SelectedRole.AccessibleModules.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            CustomerManagementPermission = modules.Contains(SystemModules.CustomerManagement);
            ProductManagementPermission = modules.Contains(SystemModules.ProductManagement);
            OrderManagementPermission = modules.Contains(SystemModules.OrderManagement);
            SalesFollowUpPermission = modules.Contains(SystemModules.SalesFollowUp);
            AfterSalesServicePermission = modules.Contains(SystemModules.AfterSalesService);
            FinanceManagementPermission = modules.Contains(SystemModules.FinanceManagement);
            SystemSettingsPermission = modules.Contains(SystemModules.SystemSettings);
        }

        private async Task SavePermissionAsync()
        {
            try
            {
                IsSaving = true;

                // 验证输入
                if (SelectedDepartment == null)
                {
                    MessageBox.Show("请选择部门", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedRole == null)
                {
                    MessageBox.Show("请选择岗位职务", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 收集选中的权限模块
                var selectedModules = new List<string>();
                if (CustomerManagementPermission) selectedModules.Add(SystemModules.CustomerManagement);
                if (ProductManagementPermission) selectedModules.Add(SystemModules.ProductManagement);
                if (OrderManagementPermission) selectedModules.Add(SystemModules.OrderManagement);
                if (SalesFollowUpPermission) selectedModules.Add(SystemModules.SalesFollowUp);
                if (AfterSalesServicePermission) selectedModules.Add(SystemModules.AfterSalesService);
                if (FinanceManagementPermission) selectedModules.Add(SystemModules.FinanceManagement);
                if (SystemSettingsPermission) selectedModules.Add(SystemModules.SystemSettings);

                // 创建角色DTO
                var roleDto = new RoleUpsertDto
                {
                    Name = SelectedRole.Name,
                    AccessibleModules = selectedModules
                };

                // 调用API创建角色
                await _apiService.CreateRoleAsync(roleDto);

                // 通知保存成功
                PermissionSaved?.Invoke(this, EventArgs.Empty);
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存权限设置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
