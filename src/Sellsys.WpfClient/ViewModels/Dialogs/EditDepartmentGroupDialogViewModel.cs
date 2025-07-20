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
    public class EditDepartmentGroupDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly DepartmentGroup _originalGroup;
        private ObservableCollection<Department> _departments;
        private Department? _selectedDepartment;
        private string _groupName = string.Empty;
        private bool _isSaving = false;
        private bool _isLoading = false;

        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        public Department? SelectedDepartment
        {
            get => _selectedDepartment;
            set => SetProperty(ref _selectedDepartment, value);
        }

        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
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
        public event EventHandler? GroupSaved;
        public event EventHandler? Cancelled;

        public EditDepartmentGroupDialogViewModel(ApiService apiService, DepartmentGroup group)
        {
            _apiService = apiService;
            _originalGroup = group;
            _departments = new ObservableCollection<Department>();

            // Initialize with current values
            GroupName = group.Name;

            SaveCommand = new AsyncRelayCommand(async p => await SaveGroupAsync(), p => CanSave());
            CancelCommand = new RelayCommand(p => Cancel());

            // Load departments when dialog opens
            _ = LoadDepartmentsAsync();
        }

        private bool CanSave()
        {
            return SelectedDepartment != null &&
                   !string.IsNullOrWhiteSpace(GroupName) &&
                   !IsSaving &&
                   !IsLoading &&
                   HasChanges();
        }

        private bool HasChanges()
        {
            return GroupName.Trim() != _originalGroup.Name ||
                   SelectedDepartment?.Id != _originalGroup.DepartmentId;
        }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                IsLoading = true;
                var departments = await _apiService.GetDepartmentsAsync();
                
                Departments.Clear();
                foreach (var department in departments)
                {
                    Departments.Add(department);
                }

                // Set the current department as selected
                SelectedDepartment = Departments.FirstOrDefault(d => d.Id == _originalGroup.DepartmentId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载部门数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveGroupAsync()
        {
            try
            {
                IsSaving = true;

                // Validate input
                if (SelectedDepartment == null)
                {
                    MessageBox.Show("请选择所属部门", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(GroupName))
                {
                    MessageBox.Show("请输入分组名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var trimmedName = GroupName.Trim();
                if (!HasChanges())
                {
                    MessageBox.Show("分组信息未发生变化", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Create update DTO
                var groupDto = new DepartmentGroupUpsertDto
                {
                    Name = trimmedName,
                    DepartmentId = SelectedDepartment.Id
                };

                // Call API to update group
                await _apiService.UpdateDepartmentGroupAsync(_originalGroup.Id, groupDto);

                // Notify success
                GroupSaved?.Invoke(this, EventArgs.Empty);
                RequestClose?.Invoke(this, EventArgs.Empty);

                MessageBox.Show("部门分组信息更新成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新部门分组信息失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
