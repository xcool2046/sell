using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class EditOrderDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Customer _customer;
        private readonly Order _originalOrder;
        private string _orderNumber = string.Empty;
        private DateTime _effectiveDate = DateTime.Now;
        private DateTime _expiryDate = DateTime.Now.AddYears(1);
        private string _salesPersonName = string.Empty;
        private string _remarks = string.Empty;
        private DateTime _createdAt = DateTime.Now;
        
        // Order Status
        private bool _isPendingPayment = true;
        private bool _isPaid;

        public EditOrderDialogViewModel(Customer customer, Order order)
        {
            _customer = customer ?? throw new ArgumentNullException(nameof(customer));
            _originalOrder = order ?? throw new ArgumentNullException(nameof(order));
            _apiService = new ApiService();

            // Initialize commands
            CancelCommand = new RelayCommand(p => Cancel());
            SaveCommand = new RelayCommand(p => Save());

            // Initialize data from order
            InitializeFromOrder();
        }

        public string CustomerName => _customer.Name;

        public string OrderNumber
        {
            get => _orderNumber;
            set => SetProperty(ref _orderNumber, value);
        }

        public DateTime EffectiveDate
        {
            get => _effectiveDate;
            set => SetProperty(ref _effectiveDate, value);
        }

        public DateTime ExpiryDate
        {
            get => _expiryDate;
            set => SetProperty(ref _expiryDate, value);
        }

        public string SalesPersonName
        {
            get => _salesPersonName;
            set => SetProperty(ref _salesPersonName, value);
        }

        public string Remarks
        {
            get => _remarks;
            set => SetProperty(ref _remarks, value);
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        // Order Status Properties
        public bool IsPendingPayment
        {
            get => _isPendingPayment;
            set => SetProperty(ref _isPendingPayment, value);
        }

        public bool IsPaid
        {
            get => _isPaid;
            set => SetProperty(ref _isPaid, value);
        }

        // Commands
        public ICommand CancelCommand { get; }
        public ICommand SaveCommand { get; }

        // Events
        public event EventHandler? CloseRequested;
        public event EventHandler? SaveCompleted;

        private void InitializeFromOrder()
        {
            try
            {
                OrderNumber = _originalOrder.OrderNumber;
                EffectiveDate = _originalOrder.EffectiveDate ?? DateTime.Now;
                ExpiryDate = _originalOrder.ExpiryDate ?? DateTime.Now.AddYears(1);
                SalesPersonName = _originalOrder.SalesPersonName ?? string.Empty;
                CreatedAt = _originalOrder.CreatedAt;
                
                // Set order status
                SetOrderStatus(_originalOrder.Status ?? "待收款");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化订单数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetOrderStatus(string status)
        {
            // Reset all status flags
            IsPendingPayment = false;
            IsPaid = false;

            // Set the appropriate flag
            switch (status)
            {
                case "已收款":
                    IsPaid = true;
                    break;
                case "待收款":
                default:
                    IsPendingPayment = true;
                    break;
            }
        }

        private async void Save()
        {
            try
            {
                // Validate required fields
                if (EffectiveDate >= ExpiryDate)
                {
                    MessageBox.Show("到期日期必须晚于生效日期", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(SalesPersonName))
                {
                    MessageBox.Show("请输入销售姓名", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get selected status
                string orderStatus = GetSelectedOrderStatus();

                // Update the original order
                _originalOrder.EffectiveDate = EffectiveDate;
                _originalOrder.ExpiryDate = ExpiryDate;
                _originalOrder.SalesPersonName = SalesPersonName;
                _originalOrder.Status = orderStatus;

                // Call API to update the order
                var orderDto = new OrderUpsertDto
                {
                    CustomerId = _originalOrder.CustomerId,
                    EffectiveDate = EffectiveDate,
                    ExpiryDate = ExpiryDate,
                    Status = orderStatus,
                    SalesPersonId = _originalOrder.SalesPersonId,
                    PaymentReceivedDate = orderStatus == "已收款" ? DateTime.Now : null,
                    OrderItems = _originalOrder.OrderItems.Select(item => new OrderItemUpsertDto
                    {
                        ProductId = item.ProductId,
                        ActualPrice = item.ActualPrice,
                        Quantity = item.Quantity,
                        TotalAmount = item.ActualPrice * item.Quantity
                    }).ToList()
                };

                await _apiService.UpdateOrderAsync(_originalOrder.Id, orderDto);
                MessageBox.Show("订单更新成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                
                SaveCompleted?.Invoke(this, EventArgs.Empty);
                Cancel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新订单失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetSelectedOrderStatus()
        {
            if (IsPaid) return "已收款";
            return "待收款";
        }

        private void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
