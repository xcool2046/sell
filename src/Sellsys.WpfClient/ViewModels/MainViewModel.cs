using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Services;
using System.Windows.Input;

namespace Sellsys.WpfClient.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase? _currentView;
        private ViewModelBase? _dialogViewModel;
        private string _currentViewName = "ProductManagement";

        public ViewModelBase? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ViewModelBase? DialogViewModel
        {
            get => _dialogViewModel;
            set => SetProperty(ref _dialogViewModel, value);
        }

        public string CurrentViewName
        {
            get => _currentViewName;
            set => SetProperty(ref _currentViewName, value);
        }

        // ViewModels for each module
        public ProductManagementViewModel ProductManagementVM { get; set; }
        public CustomerManagementViewModel CustomerManagementVM { get; set; }
        public SalesManagementViewModel SalesManagementVM { get; set; }
        public OrderManagementViewModel OrderManagementVM { get; set; }
        public AfterSalesViewModel AfterSalesVM { get; set; }
        public FinanceManagementViewModel FinanceManagementVM { get; set; }
        public SystemSettingsViewModel SystemSettingsVM { get; set; }

        // Commands to switch views
        public ICommand ShowCustomerManagementViewCommand { get; }
        public ICommand ShowSalesManagementViewCommand { get; }
        public ICommand ShowOrderManagementViewCommand { get; }
        public ICommand ShowAfterSalesViewCommand { get; }
        public ICommand ShowProductManagementViewCommand { get; }
        public ICommand ShowFinanceManagementViewCommand { get; }
        public ICommand ShowSystemSettingsViewCommand { get; }

        public MainViewModel()
        {
            // Instantiate ViewModels
            ProductManagementVM = new ProductManagementViewModel();
            CustomerManagementVM = new CustomerManagementViewModel();
            SalesManagementVM = new SalesManagementViewModel();
            OrderManagementVM = new OrderManagementViewModel();
            AfterSalesVM = new AfterSalesViewModel();
            FinanceManagementVM = new FinanceManagementViewModel();
            SystemSettingsVM = new SystemSettingsViewModel();

            // Instantiate Commands
            ShowCustomerManagementViewCommand = new RelayCommand(p => SwitchView("CustomerManagement", CustomerManagementVM));
            ShowSalesManagementViewCommand = new RelayCommand(p => SwitchView("SalesManagement", SalesManagementVM));
            ShowOrderManagementViewCommand = new RelayCommand(p => SwitchView("OrderManagement", OrderManagementVM));
            ShowAfterSalesViewCommand = new RelayCommand(p => SwitchView("AfterSales", AfterSalesVM));
            ShowProductManagementViewCommand = new RelayCommand(p => SwitchView("ProductManagement", ProductManagementVM));
            ShowFinanceManagementViewCommand = new RelayCommand(p => SwitchView("FinanceManagement", FinanceManagementVM));
            ShowSystemSettingsViewCommand = new RelayCommand(p => SwitchView("SystemSettings", SystemSettingsVM));

            // Set the initial view to Product Management for testing
            CurrentView = ProductManagementVM;
            CurrentViewName = "ProductManagement";
        }

        private async void SwitchView(string viewName, ViewModelBase viewModel)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Switching to view: {viewName}");
                CurrentViewName = viewName;
                CurrentView = viewModel;

                // Load data for the view if not already loaded
                // Use async/await properly without Task.Run to avoid thread issues
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Loading data for view: {viewName}");
                    await viewModel.LoadDataAsync();
                    System.Diagnostics.Debug.WriteLine($"Data loaded successfully for view: {viewName}");
                }
                catch (Exception ex)
                {
                    // Handle errors gracefully using the error handling service
                    ErrorHandlingService.HandleApiError(ex, $"loading data for {viewName}");
                }
            }
            catch (Exception ex)
            {
                ErrorHandlingService.HandleError(ex, $"SwitchView for {viewName}");
            }
        }

        private void SwitchView(ViewModelBase viewModel)
        {
            CurrentView = viewModel;
        }

        // Methods to be called by other ViewModels to show/hide dialogs
        public void ShowDialog(ViewModelBase viewModel)
        {
            DialogViewModel = viewModel;
        }

        public void CloseDialog()
        {
            DialogViewModel = null;
        }
    }
}