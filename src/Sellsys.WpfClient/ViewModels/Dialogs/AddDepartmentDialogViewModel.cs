using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Sellsys.WpfClient.ViewModels.Dialogs
{
    public class AddDepartmentDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
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

        public AddDepartmentDialogViewModel(ApiService apiService)
        {
            _apiService = apiService;

            SaveCommand = new AsyncRelayCommand(async p => await SaveDepartmentAsync(), p => CanSave());
            CancelCommand = new RelayCommand(p => Cancel());
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(DepartmentName) && !IsSaving;
        }

        private async Task SaveDepartmentAsync()
        {
            try
            {
                IsSaving = true;

                // 验证输入
                if (string.IsNullOrWhiteSpace(DepartmentName))
                {
                    MessageBox.Show("请输入部门名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (DepartmentName.Trim().Length > 100)
                {
                    MessageBox.Show("部门名称不能超过100个字符", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 调用API创建部门
                await _apiService.CreateDepartmentAsync(DepartmentName.Trim());

                // 通知保存成功
                DepartmentSaved?.Invoke(this, EventArgs.Empty);
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加部门失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
