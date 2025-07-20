using System.ComponentModel;

namespace Sellsys.WpfClient.Models
{
    /// <summary>
    /// 财务管理筛选条件模型
    /// </summary>
    public class FinanceFilter : INotifyPropertyChanged
    {
        private int? _customerId;
        private int? _productId;
        private DateTime? _effectiveDateStart;
        private DateTime? _effectiveDateEnd;
        private DateTime? _expiryDateStart;
        private DateTime? _expiryDateEnd;
        private DateTime? _paymentDateStart;
        private DateTime? _paymentDateEnd;
        private int? _salesPersonId;
        private string? _orderStatus;
        private string? _searchKeyword;
        private int _pageNumber = 1;
        private int _pageSize = 50;

        /// <summary>
        /// 客户ID
        /// </summary>
        public int? CustomerId
        {
            get => _customerId;
            set
            {
                _customerId = value;
                OnPropertyChanged(nameof(CustomerId));
            }
        }

        /// <summary>
        /// 产品ID
        /// </summary>
        public int? ProductId
        {
            get => _productId;
            set
            {
                _productId = value;
                OnPropertyChanged(nameof(ProductId));
            }
        }

        /// <summary>
        /// 生效日期开始
        /// </summary>
        public DateTime? EffectiveDateStart
        {
            get => _effectiveDateStart;
            set
            {
                _effectiveDateStart = value;
                OnPropertyChanged(nameof(EffectiveDateStart));
            }
        }

        /// <summary>
        /// 生效日期结束
        /// </summary>
        public DateTime? EffectiveDateEnd
        {
            get => _effectiveDateEnd;
            set
            {
                _effectiveDateEnd = value;
                OnPropertyChanged(nameof(EffectiveDateEnd));
            }
        }

        /// <summary>
        /// 到期日期开始
        /// </summary>
        public DateTime? ExpiryDateStart
        {
            get => _expiryDateStart;
            set
            {
                _expiryDateStart = value;
                OnPropertyChanged(nameof(ExpiryDateStart));
            }
        }

        /// <summary>
        /// 到期日期结束
        /// </summary>
        public DateTime? ExpiryDateEnd
        {
            get => _expiryDateEnd;
            set
            {
                _expiryDateEnd = value;
                OnPropertyChanged(nameof(ExpiryDateEnd));
            }
        }

        /// <summary>
        /// 支付日期开始
        /// </summary>
        public DateTime? PaymentDateStart
        {
            get => _paymentDateStart;
            set
            {
                _paymentDateStart = value;
                OnPropertyChanged(nameof(PaymentDateStart));
            }
        }

        /// <summary>
        /// 支付日期结束
        /// </summary>
        public DateTime? PaymentDateEnd
        {
            get => _paymentDateEnd;
            set
            {
                _paymentDateEnd = value;
                OnPropertyChanged(nameof(PaymentDateEnd));
            }
        }

        /// <summary>
        /// 负责人ID（销售人员）
        /// </summary>
        public int? SalesPersonId
        {
            get => _salesPersonId;
            set
            {
                _salesPersonId = value;
                OnPropertyChanged(nameof(SalesPersonId));
            }
        }

        /// <summary>
        /// 订单状态
        /// </summary>
        public string? OrderStatus
        {
            get => _orderStatus;
            set
            {
                _orderStatus = value;
                OnPropertyChanged(nameof(OrderStatus));
            }
        }

        /// <summary>
        /// 搜索关键词（客户名称、订单号等）
        /// </summary>
        public string? SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                OnPropertyChanged(nameof(SearchKeyword));
            }
        }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageNumber
        {
            get => _pageNumber;
            set
            {
                _pageNumber = value;
                OnPropertyChanged(nameof(PageNumber));
            }
        }

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                OnPropertyChanged(nameof(PageSize));
            }
        }

        /// <summary>
        /// 重置所有筛选条件
        /// </summary>
        public void Reset()
        {
            CustomerId = null;
            ProductId = null;
            EffectiveDateStart = null;
            EffectiveDateEnd = null;
            ExpiryDateStart = null;
            ExpiryDateEnd = null;
            PaymentDateStart = null;
            PaymentDateEnd = null;
            SalesPersonId = null;
            OrderStatus = null;
            SearchKeyword = null;
            PageNumber = 1;
        }

        /// <summary>
        /// 检查是否有任何筛选条件
        /// </summary>
        public bool HasAnyFilter => 
            CustomerId.HasValue ||
            ProductId.HasValue ||
            EffectiveDateStart.HasValue ||
            EffectiveDateEnd.HasValue ||
            ExpiryDateStart.HasValue ||
            ExpiryDateEnd.HasValue ||
            PaymentDateStart.HasValue ||
            PaymentDateEnd.HasValue ||
            SalesPersonId.HasValue ||
            !string.IsNullOrEmpty(OrderStatus) ||
            !string.IsNullOrEmpty(SearchKeyword);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 筛选选项模型
    /// </summary>
    public class FilterOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool IsSelected { get; set; }

        public override string ToString() => Text;
    }

    /// <summary>
    /// 筛选数据源模型
    /// </summary>
    public class FinanceFilterOptions
    {
        public List<FilterOption> Customers { get; set; } = new List<FilterOption>();
        public List<FilterOption> Products { get; set; } = new List<FilterOption>();
        public List<FilterOption> SalesPersons { get; set; } = new List<FilterOption>();
        public List<FilterOption> OrderStatuses { get; set; } = new List<FilterOption>();
        public List<FilterOption> EffectiveDateOptions { get; set; } = new List<FilterOption>();
        public List<FilterOption> ExpiryDateOptions { get; set; } = new List<FilterOption>();
        public List<FilterOption> PaymentDateOptions { get; set; } = new List<FilterOption>();
    }

    /// <summary>
    /// 更新收款信息模型
    /// </summary>
    public class UpdatePaymentInfo
    {
        public int OrderId { get; set; }
        public decimal ReceivedAmount { get; set; }
        public DateTime? PaymentReceivedDate { get; set; }
        public string? Remarks { get; set; }
    }
}
