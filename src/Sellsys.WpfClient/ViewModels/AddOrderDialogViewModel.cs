using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class AddOrderDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Customer _customer;
        private string _orderNumber = string.Empty;
        private string _selectedProductName = string.Empty;
        private string _productSpecification = string.Empty;
        private string _productUnit = string.Empty;
        private decimal _productListPrice;
        private decimal _actualPrice;
        private int _quantity = 1;
        private decimal _totalAmount;
        private DateTime? _effectiveDate = DateTime.Now;
        private DateTime? _expiryDate = DateTime.Now.AddYears(1);
        private string _salesPersonName = "张飞"; // TODO: Get current user
        private DateTime _signedTime = DateTime.Now;
        private bool _isPendingPayment = true;
        private bool _isPaid;
        private Product? _selectedProduct;
        private ObservableCollection<Employee> _salesPersons = new();
        private Employee? _selectedSalesPerson;

        public AddOrderDialogViewModel(Customer customer)
        {
            _customer = customer ?? throw new ArgumentNullException(nameof(customer));
            _apiService = new ApiService();

            // Initialize commands
            SelectProductCommand = new RelayCommand(p => SelectProduct());
            CancelCommand = new RelayCommand(p => Cancel());
            SaveCommand = new RelayCommand(p => Save());

            // Generate order number
            GenerateOrderNumber();

            // Load sales persons
            LoadSalesPersons();
        }

        public string CustomerName => _customer.Name;

        public string OrderNumber
        {
            get => _orderNumber;
            set => SetProperty(ref _orderNumber, value);
        }

        public string SelectedProductName
        {
            get => _selectedProductName;
            set => SetProperty(ref _selectedProductName, value);
        }

        public string ProductSpecification
        {
            get => _productSpecification;
            set => SetProperty(ref _productSpecification, value);
        }

        public string ProductUnit
        {
            get => _productUnit;
            set => SetProperty(ref _productUnit, value);
        }

        public decimal ProductListPrice
        {
            get => _productListPrice;
            set => SetProperty(ref _productListPrice, value);
        }

        public decimal ActualPrice
        {
            get => _actualPrice;
            set
            {
                if (SetProperty(ref _actualPrice, value))
                {
                    CalculateTotalAmount();
                }
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (SetProperty(ref _quantity, value))
                {
                    CalculateTotalAmount();
                }
            }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
        }

        public DateTime? EffectiveDate
        {
            get => _effectiveDate;
            set => SetProperty(ref _effectiveDate, value);
        }

        public DateTime? ExpiryDate
        {
            get => _expiryDate;
            set => SetProperty(ref _expiryDate, value);
        }

        public string SalesPersonName
        {
            get => _salesPersonName;
            set => SetProperty(ref _salesPersonName, value);
        }

        public DateTime SignedTime
        {
            get => _signedTime;
            set => SetProperty(ref _signedTime, value);
        }

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

        public ObservableCollection<Employee> SalesPersons
        {
            get => _salesPersons;
            set => SetProperty(ref _salesPersons, value);
        }

        public Employee? SelectedSalesPerson
        {
            get => _selectedSalesPerson;
            set
            {
                if (SetProperty(ref _selectedSalesPerson, value))
                {
                    SalesPersonName = value?.Name ?? string.Empty;
                }
            }
        }

        // Commands
        public ICommand SelectProductCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SaveCommand { get; }

        // Events
        public event EventHandler? CloseRequested;
        public event EventHandler? SaveCompleted;

        private void GenerateOrderNumber()
        {
            OrderNumber = $"OR{DateTime.Now:yyyyMMddHHmm}";
        }

        private void SelectProduct()
        {
            try
            {
                var dialog = new Views.Dialogs.SelectProductDialog();
                var viewModel = new SelectProductDialogViewModel();
                dialog.DataContext = viewModel;

                // Set owner to main window for proper positioning
                dialog.Owner = Application.Current.MainWindow;

                viewModel.CloseRequested += (sender, args) =>
                {
                    dialog.Close();
                };

                viewModel.ProductSelected += (sender, product) =>
                {
                    // Update product information
                    _selectedProduct = product;
                    SelectedProductName = product.Name;
                    ProductSpecification = product.SpecificationDisplay;
                    ProductUnit = product.UnitDisplay;
                    ProductListPrice = product.ListPrice;

                    // Set default actual price to list price
                    ActualPrice = product.ListPrice;

                    // Recalculate total amount
                    CalculateTotalAmount();

                    dialog.Close();
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"选择产品失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateTotalAmount()
        {
            TotalAmount = ActualPrice * Quantity;
        }

        private async void Save()
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(SelectedProductName))
                {
                    MessageBox.Show("请选择产品", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ActualPrice <= 0)
                {
                    MessageBox.Show("请输入有效的实际售价", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Quantity <= 0)
                {
                    MessageBox.Show("请输入有效的销售数量", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_selectedProduct == null)
                {
                    MessageBox.Show("请选择有效的产品", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get order status
                string orderStatus = IsPaid ? "已收款" : "待收款";

                // Check if sales person is selected
                if (SelectedSalesPerson == null)
                {
                    MessageBox.Show("请选择销售人员", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create order DTO for API call
                var orderDto = new OrderUpsertDto
                {
                    CustomerId = _customer.Id,
                    EffectiveDate = EffectiveDate,
                    ExpiryDate = ExpiryDate,
                    Status = orderStatus,
                    SalesPersonId = SelectedSalesPerson.Id,
                    PaymentReceivedDate = IsPaid ? DateTime.Now : null,
                    OrderItems = new List<OrderItemUpsertDto>
                    {
                        new OrderItemUpsertDto
                        {
                            ProductId = _selectedProduct.Id,
                            ActualPrice = ActualPrice,
                            Quantity = Quantity,
                            TotalAmount = ActualPrice * Quantity
                        }
                    }
                };

                // Call API to save the order
                await _apiService.CreateOrderAsync(orderDto);
                MessageBox.Show("订单保存成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);

                SaveCompleted?.Invoke(this, EventArgs.Empty);
                Cancel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存订单失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadSalesPersons()
        {
            try
            {
                var employees = await _apiService.GetEmployeesAsync();
                SalesPersons.Clear();

                // Add all employees to the list
                foreach (var employee in employees)
                {
                    SalesPersons.Add(employee);
                }

                // Select the first sales person or the one from sales department
                var defaultSalesPerson = employees.FirstOrDefault(e => e.DepartmentName == "销售部")
                                        ?? employees.FirstOrDefault();
                if (defaultSalesPerson != null)
                {
                    SelectedSalesPerson = defaultSalesPerson;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载销售人员失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
