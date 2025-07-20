using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Constants;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Sellsys.WpfClient.ViewModels.Dialogs
{
    public class EditPermissionDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Role _originalRole;
        private string _roleName = string.Empty;
        private bool _customerManagementPermission = false;
        private bool _productManagementPermission = false;
        private bool _orderManagementPermission = false;
        private bool _salesFollowUpPermission = false;
        private bool _afterSalesServicePermission = false;
        private bool _financeManagementPermission = false;
        private bool _systemSettingsPermission = false;
        private bool _isSaving = false;

        public string RoleName
        {
            get => _roleName;
            set => SetProperty(ref _roleName, value);
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

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Events
        public event EventHandler? RequestClose;
        public event EventHandler? PermissionSaved;
        public event EventHandler? Cancelled;

        public EditPermissionDialogViewModel(ApiService apiService, Role role)
        {
            _apiService = apiService;
            _originalRole = role;

            // Initialize with current values
            RoleName = role.Name;
            LoadCurrentPermissions(role.AccessibleModules);

            SaveCommand = new AsyncRelayCommand(async p => await SavePermissionAsync(), p => CanSave());
            CancelCommand = new RelayCommand(p => Cancel());
        }

        private void LoadCurrentPermissions(string accessibleModules)
        {
            if (string.IsNullOrEmpty(accessibleModules)) return;

            var modules = accessibleModules.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                         .Select(m => m.Trim())
                                         .ToList();

            CustomerManagementPermission = modules.Contains(SystemModules.CustomerManagement);
            ProductManagementPermission = modules.Contains(SystemModules.ProductManagement);
            OrderManagementPermission = modules.Contains(SystemModules.OrderManagement);
            SalesFollowUpPermission = modules.Contains(SystemModules.SalesFollowUp);
            AfterSalesServicePermission = modules.Contains(SystemModules.AfterSalesService);
            FinanceManagementPermission = modules.Contains(SystemModules.FinanceManagement);
            SystemSettingsPermission = modules.Contains(SystemModules.SystemSettings);
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(RoleName) && 
                   !IsSaving &&
                   HasChanges();
        }

        private bool HasChanges()
        {
            if (RoleName.Trim() != _originalRole.Name)
                return true;

            var currentModules = GetSelectedModules();
            var originalModules = _originalRole.AccessibleModules?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                                  .Select(m => m.Trim())
                                                                  .OrderBy(m => m)
                                                                  .ToList() ?? new List<string>();

            var newModules = currentModules.OrderBy(m => m).ToList();

            return !originalModules.SequenceEqual(newModules);
        }

        private List<string> GetSelectedModules()
        {
            var selectedModules = new List<string>();
            if (CustomerManagementPermission) selectedModules.Add(SystemModules.CustomerManagement);
            if (ProductManagementPermission) selectedModules.Add(SystemModules.ProductManagement);
            if (OrderManagementPermission) selectedModules.Add(SystemModules.OrderManagement);
            if (SalesFollowUpPermission) selectedModules.Add(SystemModules.SalesFollowUp);
            if (AfterSalesServicePermission) selectedModules.Add(SystemModules.AfterSalesService);
            if (FinanceManagementPermission) selectedModules.Add(SystemModules.FinanceManagement);
            if (SystemSettingsPermission) selectedModules.Add(SystemModules.SystemSettings);

            return selectedModules;
        }

        private async Task SavePermissionAsync()
        {
            try
            {
                IsSaving = true;

                // Validate input
                if (string.IsNullOrWhiteSpace(RoleName))
                {
                    MessageBox.Show("请输入角色名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!HasChanges())
                {
                    MessageBox.Show("角色权限未发生变化", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Collect selected permission modules
                var selectedModules = GetSelectedModules();

                // Create role DTO
                var roleDto = new RoleUpsertDto
                {
                    Name = RoleName.Trim(),
                    AccessibleModules = selectedModules
                };

                // Call API to update role
                await _apiService.UpdateRoleAsync(_originalRole.Id, roleDto);

                // Notify success
                PermissionSaved?.Invoke(this, EventArgs.Empty);
                RequestClose?.Invoke(this, EventArgs.Empty);

                MessageBox.Show("角色权限更新成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新角色权限失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
