using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using Sellsys.WpfClient.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class OrderRecordsDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Customer _customer;
        private ObservableCollection<Order> _orderRecords;
        private Order? _selectedOrderRecord;
        private bool _isLoading;

        public OrderRecordsDialogViewModel(Customer customer)
        {
            _customer = customer ?? throw new ArgumentNullException(nameof(customer));
            _apiService = new ApiService();
            _orderRecords = new ObservableCollection<Order>();

            // Initialize commands
            AddOrderCommand = new RelayCommand(p => AddOrder());
            EditOrderCommand = new RelayCommand(p => EditOrder(p as Order));
            DeleteOrderCommand = new RelayCommand(p => DeleteOrder(p as Order));
            CloseCommand = new RelayCommand(p => Close());

            // Load data
            _ = LoadOrderRecordsAsync();
        }

        public string CustomerName => _customer.Name;

        public ObservableCollection<Order> OrderRecords
        {
            get => _orderRecords;
            set => SetProperty(ref _orderRecords, value);
        }

        public Order? SelectedOrderRecord
        {
            get => _selectedOrderRecord;
            set => SetProperty(ref _selectedOrderRecord, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Commands
        public ICommand AddOrderCommand { get; }
        public ICommand EditOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand CloseCommand { get; }

        // Events
        public event EventHandler? CloseRequested;

        private async Task LoadOrderRecordsAsync()
        {
            try
            {
                IsLoading = true;

                // Call API to get order records for this customer
                var orders = await _apiService.SearchOrdersAsync(new OrderSearchCriteria
                {
                    CustomerName = _customer.Name
                });

                OrderRecords.Clear();
                foreach (var order in orders)
                {
                    OrderRecords.Add(order);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载订单记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddOrder()
        {
            try
            {
                var dialog = new AddOrderDialog();
                var viewModel = new AddOrderDialogViewModel(_customer);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.CloseRequested += (sender, args) =>
                {
                    dialog.Close();
                };

                viewModel.SaveCompleted += async (sender, args) =>
                {
                    // Reload order records after saving
                    await LoadOrderRecordsAsync();
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加订单失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditOrder(Order? order)
        {
            if (order == null) return;

            try
            {
                var dialog = new EditOrderDialog();
                var viewModel = new EditOrderDialogViewModel(_customer, order);
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.CloseRequested += (sender, args) =>
                {
                    dialog.Close();
                };

                viewModel.SaveCompleted += async (sender, args) =>
                {
                    // Reload order records after saving
                    await LoadOrderRecordsAsync();
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"编辑订单失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteOrder(Order? order)
        {
            if (order == null) return;

            try
            {
                var result = MessageBox.Show($"确定要删除订单 {order.OrderNumber} 吗？", 
                    "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // TODO: Call API to delete the order
                    OrderRecords.Remove(order);
                    MessageBox.Show("订单已删除", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除订单失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
