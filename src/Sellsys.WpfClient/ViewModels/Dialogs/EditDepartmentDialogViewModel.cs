using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Sellsys.WpfClient.ViewModels.Dialogs
{
    public class EditDepartmentDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Department _originalDepartment;
        private string _departmentName = string.Empty;
        private bool _isSaving = false;

        public string DepartmentName
        {
            get => _departmentName;
            set => SetProperty(ref _departmentName, value);
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
        public event EventHandler? DepartmentSaved;
        public event EventHandler? Cancelled;

        public EditDepartmentDialogViewModel(ApiService apiService, Department department)
        {
            _apiService = apiService;
            _originalDepartment = department;

            // Initialize with current values
            DepartmentName = department.Name;

            SaveCommand = new AsyncRelayCommand(async p => await SaveDepartmentAsync(), p => CanSave());
            CancelCommand = new RelayCommand(p => Cancel());
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(DepartmentName) && 
                   !IsSaving &&
                   DepartmentName.Trim() != _originalDepartment.Name; // Only allow save if name changed
        }

        private async Task SaveDepartmentAsync()
        {
            try
            {
                IsSaving = true;

                // Validate input
                if (string.IsNullOrWhiteSpace(DepartmentName))
                {
                    MessageBox.Show("请输入部门名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var trimmedName = DepartmentName.Trim();
                if (trimmedName == _originalDepartment.Name)
                {
                    MessageBox.Show("部门名称未发生变化", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Create update DTO
                var departmentDto = new DepartmentUpsertDto
                {
                    Name = trimmedName
                };

                // Call API to update department
                await _apiService.UpdateDepartmentAsync(_originalDepartment.Id, departmentDto);

                // Notify success
                DepartmentSaved?.Invoke(this, EventArgs.Empty);
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新部门信息失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
