using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Services;
using Sellsys.WpfClient.Views;
using System.Windows;
using System.Windows.Input;

namespace Sellsys.WpfClient.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase? _currentView;
        private ViewModelBase? _dialogViewModel;
        private string _currentViewName = "CustomerManagement";
        private string _currentUserInfo = string.Empty;

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

        public string CurrentUserInfo
        {
            get => _currentUserInfo;
            set => SetProperty(ref _currentUserInfo, value);
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
        public ICommand LogoutCommand { get; }

        public MainViewModel()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MainViewModel: Starting initialization...");

                // Instantiate ViewModels
                System.Diagnostics.Debug.WriteLine("MainViewModel: Creating ProductManagementViewModel...");
                ProductManagementVM = new ProductManagementViewModel();

                System.Diagnostics.Debug.WriteLine("MainViewModel: Creating CustomerManagementViewModel...");
                CustomerManagementVM = new CustomerManagementViewModel();

                System.Diagnostics.Debug.WriteLine("MainViewModel: Creating SalesManagementViewModel...");
                SalesManagementVM = new SalesManagementViewModel();

                System.Diagnostics.Debug.WriteLine("MainViewModel: Creating OrderManagementViewModel...");
                OrderManagementVM = new OrderManagementViewModel();

                System.Diagnostics.Debug.WriteLine("MainViewModel: Creating AfterSalesViewModel...");
                AfterSalesVM = new AfterSalesViewModel();

                System.Diagnostics.Debug.WriteLine("MainViewModel: Creating FinanceManagementViewModel...");
                FinanceManagementVM = new FinanceManagementViewModel();

                System.Diagnostics.Debug.WriteLine("MainViewModel: Creating SystemSettingsViewModel...");
                SystemSettingsVM = new SystemSettingsViewModel();

                System.Diagnostics.Debug.WriteLine("MainViewModel: All ViewModels created successfully");

                // Instantiate Commands
                System.Diagnostics.Debug.WriteLine("MainViewModel: Creating commands...");
                ShowCustomerManagementViewCommand = new RelayCommand(p => SwitchView("CustomerManagement", CustomerManagementVM));
                ShowSalesManagementViewCommand = new RelayCommand(p => SwitchView("SalesManagement", SalesManagementVM));
                ShowOrderManagementViewCommand = new RelayCommand(p => SwitchView("OrderManagement", OrderManagementVM));
                ShowAfterSalesViewCommand = new RelayCommand(p => SwitchView("AfterSales", AfterSalesVM));
                ShowProductManagementViewCommand = new RelayCommand(p => SwitchView("ProductManagement", ProductManagementVM));
                ShowFinanceManagementViewCommand = new RelayCommand(p => SwitchView("FinanceManagement", FinanceManagementVM));
                ShowSystemSettingsViewCommand = new RelayCommand(p => SwitchView("SystemSettings", SystemSettingsVM));
                LogoutCommand = new RelayCommand(p => Logout());

                System.Diagnostics.Debug.WriteLine("MainViewModel: Commands created successfully");

                // 订阅用户变更事件
                CurrentUser.UserChanged += OnUserChanged;

                // 初始化用户信息
                UpdateCurrentUserInfo();

                // Set the initial view based on user permissions
                System.Diagnostics.Debug.WriteLine("MainViewModel: Setting initial view based on permissions...");
                SetInitialViewBasedOnPermissions();

                System.Diagnostics.Debug.WriteLine("MainViewModel: Initialization completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainViewModel: Exception during initialization: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"MainViewModel: Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async void SwitchView(string viewName, ViewModelBase viewModel)
        {
            try
            {
                // 检查权限（除了基于权限的初始设置）
                string modulePermission = GetModulePermissionName(viewName);
                if (!string.IsNullOrEmpty(modulePermission) && !HasPermission(modulePermission))
                {
                    MessageBox.Show("您没有权限访问此模块", "权限不足", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

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

        /// <summary>
        /// 获取视图对应的权限模块名称
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <returns>权限模块名称</returns>
        private string GetModulePermissionName(string viewName)
        {
            return viewName switch
            {
                "CustomerManagement" => Constants.SystemModules.CustomerManagement,
                "SalesManagement" => Constants.SystemModules.SalesFollowUp,
                "OrderManagement" => Constants.SystemModules.OrderManagement,
                "AfterSales" => Constants.SystemModules.AfterSalesService,
                "ProductManagement" => Constants.SystemModules.ProductManagement,
                "FinanceManagement" => Constants.SystemModules.FinanceManagement,
                "SystemSettings" => Constants.SystemModules.SystemSettings,
                _ => string.Empty
            };
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

        /// <summary>
        /// 用户信息变更事件处理
        /// </summary>
        /// <param name="userInfo">用户信息</param>
        private void OnUserChanged(Models.UserInfo? userInfo)
        {
            UpdateCurrentUserInfo();
        }

        /// <summary>
        /// 更新当前用户信息显示
        /// </summary>
        private void UpdateCurrentUserInfo()
        {
            CurrentUserInfo = CurrentUser.GetUserDisplayInfo();
        }

        /// <summary>
        /// 登出
        /// </summary>
        private void Logout()
        {
            try
            {
                var result = MessageBox.Show("确定要退出登录吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // 清除当前用户
                    CurrentUser.ClearUser();

                    // 先创建并显示登录窗口
                    var loginWindow = new Views.LoginWindow();

                    // 设置登录窗口为应用程序主窗口
                    Application.Current.MainWindow = loginWindow;
                    loginWindow.Show();

                    // 激活登录窗口
                    loginWindow.Activate();
                    loginWindow.Focus();

                    // 然后关闭主窗口
                    var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    if (mainWindow != null)
                    {
                        mainWindow.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"登出过程中发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 检查用户是否有权限访问指定模块
        /// </summary>
        /// <param name="module">模块名称</param>
        /// <returns>是否有权限</returns>
        public bool HasPermission(string module)
        {
            return CurrentUser.HasPermission(module);
        }

        /// <summary>
        /// 根据权限设置初始视图
        /// </summary>
        private void SetInitialViewBasedOnPermissions()
        {
            // 按优先级顺序检查权限并设置初始视图（同步版本，不加载数据）
            if (HasPermission(Constants.SystemModules.CustomerManagement))
            {
                SwitchViewSync("CustomerManagement", CustomerManagementVM);
            }
            else if (HasPermission(Constants.SystemModules.SalesFollowUp))
            {
                SwitchViewSync("SalesManagement", SalesManagementVM);
            }
            else if (HasPermission(Constants.SystemModules.OrderManagement))
            {
                SwitchViewSync("OrderManagement", OrderManagementVM);
            }
            else if (HasPermission(Constants.SystemModules.ProductManagement))
            {
                SwitchViewSync("ProductManagement", ProductManagementVM);
            }
            else if (HasPermission(Constants.SystemModules.AfterSalesService))
            {
                SwitchViewSync("AfterSales", AfterSalesVM);
            }
            else if (HasPermission(Constants.SystemModules.FinanceManagement))
            {
                SwitchViewSync("FinanceManagement", FinanceManagementVM);
            }
            else if (HasPermission(Constants.SystemModules.SystemSettings))
            {
                SwitchViewSync("SystemSettings", SystemSettingsVM);
            }
            else
            {
                // 如果没有任何权限，显示一个空白页面或错误信息
                CurrentView = null;
                CurrentViewName = "NoPermission";
            }
        }

        /// <summary>
        /// 同步切换视图（不加载数据）
        /// </summary>
        private void SwitchViewSync(string viewName, ViewModelBase viewModel)
        {
            System.Diagnostics.Debug.WriteLine($"Switching to view (sync): {viewName}");
            CurrentViewName = viewName;
            CurrentView = viewModel;
        }
    }
}