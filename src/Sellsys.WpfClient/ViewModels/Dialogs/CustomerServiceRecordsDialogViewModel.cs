using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using Sellsys.WpfClient.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels.Dialogs
{
    public class CustomerServiceRecordsDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Customer _customer;
        private readonly List<Employee> _employees;
        private ObservableCollection<AfterSalesRecord> _records;
        private AfterSalesRecord? _selectedRecord;
        private bool _isLoading;

        public ObservableCollection<AfterSalesRecord> Records
        {
            get => _records;
            set => SetProperty(ref _records, value);
        }

        public AfterSalesRecord? SelectedRecord
        {
            get => _selectedRecord;
            set => SetProperty(ref _selectedRecord, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // 客户信息
        public string CustomerName => _customer.Name;
        public string CustomerUnit => _customer.Name; // 客户单位
        public string? CustomerProvince => _customer.Province;
        public string? CustomerCity => _customer.City;
        public string? CustomerAddress => _customer.Address;
        
        // 主要联系人信息
        public string? PrimaryContactName => _customer.Contacts?.FirstOrDefault(c => c.IsPrimary)?.Name;
        public string? PrimaryContactPhone => _customer.Contacts?.FirstOrDefault(c => c.IsPrimary)?.Phone;

        // Commands
        public ICommand LoadRecordsCommand { get; }
        public ICommand AddRecordCommand { get; }
        public ICommand EditRecordCommand { get; }
        public ICommand DeleteRecordCommand { get; }
        public ICommand CloseCommand { get; }
        
        // Row-level commands
        public ICommand EditRecordRowCommand { get; }
        public ICommand DeleteRecordRowCommand { get; }

        public event EventHandler? RequestClose;
        public event EventHandler? RecordChanged;

        public CustomerServiceRecordsDialogViewModel(ApiService apiService, Customer customer, List<Employee> employees)
        {
            _apiService = apiService;
            _customer = customer;
            _employees = employees;
            _records = new ObservableCollection<AfterSalesRecord>();

            // Initialize commands
            LoadRecordsCommand = new AsyncRelayCommand(async p => await LoadRecordsAsync());
            AddRecordCommand = new RelayCommand(p => AddRecord());
            EditRecordCommand = new RelayCommand(p => EditRecord(), p => SelectedRecord != null);
            DeleteRecordCommand = new AsyncRelayCommand(async p => await DeleteRecordAsync(), p => SelectedRecord != null);
            CloseCommand = new RelayCommand(p => Close());
            
            // Row-level commands
            EditRecordRowCommand = new RelayCommand(p => EditRecordRow(p as AfterSalesRecord), p => p is AfterSalesRecord);
            DeleteRecordRowCommand = new AsyncRelayCommand(async p => await DeleteRecordRowAsync(p as AfterSalesRecord), p => p is AfterSalesRecord);

            // Load data when initialized
            _ = LoadRecordsAsync();
        }

        private async Task LoadRecordsAsync()
        {
            try
            {
                IsLoading = true;
                var records = await _apiService.GetAfterSalesRecordsByCustomerIdAsync(_customer.Id);
                
                Records.Clear();
                foreach (var record in records.OrderByDescending(r => r.CreatedAt))
                {
                    Records.Add(record);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载客服记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddRecord()
        {
            var dialogViewModel = new AddEditFeedbackDialogViewModel(_apiService, _customer, _employees);
            var dialog = new AddEditFeedbackDialog(dialogViewModel);

            // 订阅记录保存事件
            dialogViewModel.RecordSaved += async (sender, e) =>
            {
                await LoadRecordsAsync();
                RecordChanged?.Invoke(this, EventArgs.Empty);
            };

            dialog.ShowDialog();
        }

        private void EditRecord()
        {
            if (SelectedRecord == null) return;
            EditRecordRow(SelectedRecord);
        }

        private void EditRecordRow(AfterSalesRecord? record)
        {
            if (record == null) return;

            var dialogViewModel = new AddEditFeedbackDialogViewModel(_apiService, _customer, _employees, record);
            var dialog = new AddEditFeedbackDialog(dialogViewModel);

            // 订阅记录保存事件
            dialogViewModel.RecordSaved += async (sender, e) =>
            {
                await LoadRecordsAsync();
                RecordChanged?.Invoke(this, EventArgs.Empty);
            };

            dialog.ShowDialog();
        }

        private async Task DeleteRecordAsync()
        {
            if (SelectedRecord == null) return;
            await DeleteRecordRowAsync(SelectedRecord);
        }

        private async Task DeleteRecordRowAsync(AfterSalesRecord? record)
        {
            if (record == null) return;

            var result = MessageBox.Show(
                $"确定要删除这条售后记录吗？此操作不可撤销。",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _apiService.DeleteAfterSalesRecordAsync(record.Id);
                    await LoadRecordsAsync();
                    RecordChanged?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除售后记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private void Close()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
