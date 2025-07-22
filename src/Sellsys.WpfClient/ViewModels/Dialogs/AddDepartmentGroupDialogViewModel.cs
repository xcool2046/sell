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
    public class AddDepartmentGroupDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
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
        public event EventHandler? DepartmentGroupSaved;
        public event EventHandler? Cancelled;

        public AddDepartmentGroupDialogViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _departments = new ObservableCollection<Department>();

            SaveCommand = new AsyncRelayCommand(async p => await SaveDepartmentGroupAsync(), p => CanSave());
            CancelCommand = new RelayCommand(p => Cancel());

            // Load departments when dialog opens
            _ = LoadDepartmentsAsync();
        }

        private bool CanSave()
        {
            return SelectedDepartment != null && 
                   !string.IsNullOrWhiteSpace(GroupName) && 
                   !IsSaving && 
                   !IsLoading;
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

                // 如果只有一个部门，自动选择
                if (Departments.Count == 1)
                {
                    SelectedDepartment = Departments.First();
                }
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

        private async Task SaveDepartmentGroupAsync()
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

                if (string.IsNullOrWhiteSpace(GroupName))
                {
                    MessageBox.Show("请输入分组名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (GroupName.Trim().Length > 100)
                {
                    MessageBox.Show("分组名称不能超过100个字符", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 调用API创建部门分组
                await _apiService.CreateDepartmentGroupAsync(SelectedDepartment.Id, GroupName.Trim());

                // 通知保存成功
                DepartmentGroupSaved?.Invoke(this, EventArgs.Empty);
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加部门分组失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
