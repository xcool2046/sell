using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using Sellsys.WpfClient.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class ContactRecordsDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Customer _customer;
        private ObservableCollection<SalesFollowUpLog> _contactRecords;
        private SalesFollowUpLog? _selectedContactRecord;
        private bool _isLoading;

        public ContactRecordsDialogViewModel(Customer customer)
        {
            _customer = customer ?? throw new ArgumentNullException(nameof(customer));
            _apiService = new ApiService();
            _contactRecords = new ObservableCollection<SalesFollowUpLog>();

            // Initialize commands
            AddContactRecordCommand = new RelayCommand(p => AddContactRecord());
            EditContactRecordCommand = new RelayCommand(p => EditContactRecord(p as SalesFollowUpLog));
            DeleteContactRecordCommand = new RelayCommand(p => DeleteContactRecord(p as SalesFollowUpLog));
            CloseCommand = new RelayCommand(p => Close());

            // Load data
            _ = LoadContactRecordsAsync();
        }

        public string CustomerName => _customer.Name;

        public ObservableCollection<SalesFollowUpLog> ContactRecords
        {
            get => _contactRecords;
            set => SetProperty(ref _contactRecords, value);
        }

        public SalesFollowUpLog? SelectedContactRecord
        {
            get => _selectedContactRecord;
            set => SetProperty(ref _selectedContactRecord, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Commands
        public ICommand AddContactRecordCommand { get; }
        public ICommand EditContactRecordCommand { get; }
        public ICommand DeleteContactRecordCommand { get; }
        public ICommand CloseCommand { get; }

        // Events
        public event EventHandler? CloseRequested;

        private async Task LoadContactRecordsAsync()
        {
            try
            {
                IsLoading = true;

                // Call API to get contact records for this customer
                var records = await _apiService.GetSalesFollowUpLogsByCustomerIdAsync(_customer.Id);

                ContactRecords.Clear();
                foreach (var record in records.OrderByDescending(r => r.CreatedAt))
                {
                    ContactRecords.Add(record);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载联系记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddContactRecord()
        {
            try
            {
                var dialog = new AddContactRecordDialog();
                var viewModel = new AddContactRecordDialogViewModel(_customer);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.CloseRequested += (sender, args) =>
                {
                    dialog.Close();
                };

                viewModel.SaveCompleted += async (sender, args) =>
                {
                    // Reload contact records after saving
                    await LoadContactRecordsAsync();
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加联系记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditContactRecord(SalesFollowUpLog? record)
        {
            if (record == null) return;

            try
            {
                var dialog = new EditContactRecordDialog();
                var viewModel = new EditContactRecordDialogViewModel(_customer, record);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.CloseRequested += (sender, args) =>
                {
                    dialog.Close();
                };

                viewModel.SaveCompleted += async (sender, args) =>
                {
                    // Reload contact records after saving
                    await LoadContactRecordsAsync();
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"编辑联系记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteContactRecord(SalesFollowUpLog? record)
        {
            if (record == null) return;

            try
            {
                var result = MessageBox.Show($"确定要删除这条联系记录吗？\n\n{record.Summary}", 
                    "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Call API to delete the record
                    await _apiService.DeleteSalesFollowUpLogAsync(record.Id);
                    ContactRecords.Remove(record);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除联系记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
