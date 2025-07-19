using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    /// <summary>
    /// 财务管理ViewModel - 重构版本，适配原型图需求
    /// </summary>
    public class FinanceManagementViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;

        // 主要数据集合
        private ObservableCollection<FinanceOrderDetail> _financeOrderDetails;
        private FinanceOrderDetail? _selectedOrderDetail;
        private FinanceOrderSummary? _summary;
        private bool _isLoading;

        // 筛选相关
        private FinanceFilter _filter;
        private FinanceFilterOptions? _filterOptions;

        // 分页相关
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _totalCount = 0;

        // 搜索
        private string _searchKeyword = string.Empty;
        private string _searchCustomerName = string.Empty;

        #region 属性

        /// <summary>
        /// 财务订单明细列表
        /// </summary>
        public ObservableCollection<FinanceOrderDetail> FinanceOrderDetails
        {
            get => _financeOrderDetails;
            set => SetProperty(ref _financeOrderDetails, value);
        }

        /// <summary>
        /// 选中的订单明细
        /// </summary>
        public FinanceOrderDetail? SelectedOrderDetail
        {
            get => _selectedOrderDetail;
            set => SetProperty(ref _selectedOrderDetail, value);
        }

        /// <summary>
        /// 财务汇总信息
        /// </summary>
        public FinanceOrderSummary? Summary
        {
            get => _summary;
            set => SetProperty(ref _summary, value);
        }

        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// 筛选条件
        /// </summary>
        public FinanceFilter Filter
        {
            get => _filter;
            set => SetProperty(ref _filter, value);
        }

        /// <summary>
        /// 筛选数据源
        /// </summary>
        public FinanceFilterOptions? FilterOptions
        {
            get => _filterOptions;
            set => SetProperty(ref _filterOptions, value);
        }

        /// <summary>
        /// 搜索关键词
        /// </summary>
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                SetProperty(ref _searchKeyword, value);
                Filter.SearchKeyword = value;
            }
        }

        /// <summary>
        /// 输入客户名称
        /// </summary>
        public string SearchCustomerName
        {
            get => _searchCustomerName;
            set => SetProperty(ref _searchCustomerName, value);
        }

        /// <summary>
        /// 生效日期选项
        /// </summary>
        public List<string> EffectiveDateOptions { get; } = new List<string>
        {
            "全部", "本月", "上月", "本季度", "上季度", "本年", "去年"
        };

        /// <summary>
        /// 到期日期选项
        /// </summary>
        public List<string> ExpiryDateOptions { get; } = new List<string>
        {
            "全部", "本月", "上月", "本季度", "上季度", "本年", "去年"
        };

        /// <summary>
        /// 签单日期选项
        /// </summary>
        public List<string> CreatedDateOptions { get; } = new List<string>
        {
            "全部", "本月", "上月", "本季度", "上季度", "本年", "去年"
        };

        /// <summary>
        /// 选中的生效日期
        /// </summary>
        public string? SelectedEffectiveDate { get; set; }

        /// <summary>
        /// 选中的到期日期
        /// </summary>
        public string? SelectedExpiryDate { get; set; }

        /// <summary>
        /// 选中的签单日期
        /// </summary>
        public string? SelectedCreatedDate { get; set; }



        /// <summary>
        /// 当前页码
        /// </summary>
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                SetProperty(ref _currentPage, value);
                Filter.PageNumber = value;
            }
        }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        // 筛选条件的便捷属性
        /// <summary>
        /// 选中的客户
        /// </summary>
        public FilterOption? SelectedCustomer
        {
            get => FilterOptions?.Customers.FirstOrDefault(c => c.Value == Filter.CustomerId?.ToString());
            set
            {
                Filter.CustomerId = value != null && int.TryParse(value.Value, out var id) ? id : null;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 选中的产品
        /// </summary>
        public FilterOption? SelectedProduct
        {
            get => FilterOptions?.Products.FirstOrDefault(p => p.Value == Filter.ProductId?.ToString());
            set
            {
                Filter.ProductId = value != null && int.TryParse(value.Value, out var id) ? id : null;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 选中的负责人
        /// </summary>
        public FilterOption? SelectedSalesPerson
        {
            get => FilterOptions?.SalesPersons.FirstOrDefault(s => s.Value == Filter.SalesPersonId?.ToString());
            set
            {
                Filter.SalesPersonId = value != null && int.TryParse(value.Value, out var id) ? id : null;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 选中的订单状态
        /// </summary>
        public FilterOption? SelectedOrderStatus
        {
            get => FilterOptions?.OrderStatuses.FirstOrDefault(s => s.Value == Filter.OrderStatus);
            set
            {
                Filter.OrderStatus = value?.Value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 生效日期开始
        /// </summary>
        public DateTime? EffectiveDateStart
        {
            get => Filter.EffectiveDateStart;
            set
            {
                Filter.EffectiveDateStart = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 生效日期结束
        /// </summary>
        public DateTime? EffectiveDateEnd
        {
            get => Filter.EffectiveDateEnd;
            set
            {
                Filter.EffectiveDateEnd = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 到期日期开始
        /// </summary>
        public DateTime? ExpiryDateStart
        {
            get => Filter.ExpiryDateStart;
            set
            {
                Filter.ExpiryDateStart = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 到期日期结束
        /// </summary>
        public DateTime? ExpiryDateEnd
        {
            get => Filter.ExpiryDateEnd;
            set
            {
                Filter.ExpiryDateEnd = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 支付日期开始
        /// </summary>
        public DateTime? PaymentDateStart
        {
            get => Filter.PaymentDateStart;
            set
            {
                Filter.PaymentDateStart = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 支付日期结束
        /// </summary>
        public DateTime? PaymentDateEnd
        {
            get => Filter.PaymentDateEnd;
            set
            {
                Filter.PaymentDateEnd = value;
                OnPropertyChanged();
            }
        }

        // 分页相关属性
        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// 分页信息显示
        /// </summary>
        public string PageInfo => $"第 {CurrentPage} 页，共 {TotalPages} 页，总计 {TotalCount} 条记录";

        #endregion

        #region 命令

        public ICommand LoadDataCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand EditPaymentCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand GoToPageCommand { get; }

        #endregion

        #region 构造函数

        public FinanceManagementViewModel()
        {
            _apiService = new ApiService();
            _financeOrderDetails = new ObservableCollection<FinanceOrderDetail>();
            _filter = new FinanceFilter();

            // 初始化空的筛选选项，避免界面绑定失败
            _filterOptions = new FinanceFilterOptions
            {
                Customers = new List<FilterOption>(),
                Products = new List<FilterOption>(),
                SalesPersons = new List<FilterOption>(),
                OrderStatuses = new List<FilterOption>()
            };

            // 初始化命令
            LoadDataCommand = new AsyncRelayCommand(async p => await LoadDataAsync());
            SearchCommand = new AsyncRelayCommand(async p => await SearchAsync());
            ResetFiltersCommand = new RelayCommand(p => ResetFilters());
            EditPaymentCommand = new AsyncRelayCommand(async p => await EditPaymentAsync(), p => SelectedOrderDetail?.CanEditPayment == true);
            RefreshCommand = new AsyncRelayCommand(async p => await RefreshAsync());
            PreviousPageCommand = new RelayCommand(p => PreviousPage(), p => HasPreviousPage);
            NextPageCommand = new RelayCommand(p => NextPage(), p => HasNextPage);
            GoToPageCommand = new RelayCommand(p => GoToPage(p));
        }

        #endregion

        #region 方法实现

        public override async Task LoadDataAsync()
        {
            if (IsDataLoaded) return;
            await LoadFilterOptionsAsync();
            await LoadFinanceDataAsync();
            IsDataLoaded = true;
        }

        /// <summary>
        /// 加载筛选数据源
        /// </summary>
        private async Task LoadFilterOptionsAsync()
        {
            try
            {
                FilterOptions = await _apiService.GetFinanceFilterOptionsAsync();
            }
            catch (Exception ex)
            {
                // 提供默认的空筛选选项，避免界面绑定失败
                FilterOptions = new FinanceFilterOptions
                {
                    Customers = new List<FilterOption>(),
                    Products = new List<FilterOption>(),
                    SalesPersons = new List<FilterOption>(),
                    OrderStatuses = new List<FilterOption>()
                };

                MessageBox.Show($"加载筛选数据源失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 加载财务数据
        /// </summary>
        private async Task LoadFinanceDataAsync()
        {
            try
            {
                IsLoading = true;

                var result = await _apiService.GetFinanceOrderDetailsAsync(Filter);

                FinanceOrderDetails.Clear();
                foreach (var item in result.Items)
                {
                    FinanceOrderDetails.Add(item);
                }

                TotalCount = result.TotalCount;
                CurrentPage = result.PageNumber;
                TotalPages = result.TotalPages;
                Summary = result.Summary;

                // 通知分页相关属性更新
                OnPropertyChanged(nameof(HasPreviousPage));
                OnPropertyChanged(nameof(HasNextPage));
                OnPropertyChanged(nameof(PageInfo));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载财务数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 搜索
        /// </summary>
        private async Task SearchAsync()
        {
            Filter.PageNumber = 1; // 重置到第一页
            await LoadFinanceDataAsync();
        }

        /// <summary>
        /// 重置筛选条件
        /// </summary>
        private void ResetFilters()
        {
            Filter.Reset();
            SearchKeyword = string.Empty;

            // 通知所有筛选相关属性更新
            OnPropertyChanged(nameof(SelectedCustomer));
            OnPropertyChanged(nameof(SelectedProduct));
            OnPropertyChanged(nameof(SelectedSalesPerson));
            OnPropertyChanged(nameof(SelectedOrderStatus));
            OnPropertyChanged(nameof(EffectiveDateStart));
            OnPropertyChanged(nameof(EffectiveDateEnd));
            OnPropertyChanged(nameof(ExpiryDateStart));
            OnPropertyChanged(nameof(ExpiryDateEnd));
            OnPropertyChanged(nameof(PaymentDateStart));
            OnPropertyChanged(nameof(PaymentDateEnd));
        }

        /// <summary>
        /// 编辑收款信息
        /// </summary>
        private async Task EditPaymentAsync()
        {
            if (SelectedOrderDetail == null) return;

            try
            {
                // 创建对话框ViewModel
                var dialogViewModel = new ViewModels.Dialogs.EditPaymentInfoDialogViewModel(SelectedOrderDetail);

                // 创建并显示对话框
                var dialog = new Views.Dialogs.EditPaymentInfoDialog(dialogViewModel);
                dialog.Owner = Application.Current.MainWindow;

                var result = dialog.ShowDialog();

                // 如果保存成功，刷新数据
                if (result == true)
                {
                    await RefreshAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开编辑对话框失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        private async Task RefreshAsync()
        {
            await LoadFinanceDataAsync();
        }

        /// <summary>
        /// 上一页
        /// </summary>
        private void PreviousPage()
        {
            if (HasPreviousPage)
            {
                CurrentPage--;
                _ = LoadFinanceDataAsync();
            }
        }

        /// <summary>
        /// 下一页
        /// </summary>
        private void NextPage()
        {
            if (HasNextPage)
            {
                CurrentPage++;
                _ = LoadFinanceDataAsync();
            }
        }

        /// <summary>
        /// 跳转到指定页
        /// </summary>
        private void GoToPage(object? parameter)
        {
            if (parameter is int page && page > 0 && page <= TotalPages)
            {
                CurrentPage = page;
                _ = LoadFinanceDataAsync();
            }
        }

        #endregion
    }
}