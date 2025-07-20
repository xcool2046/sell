using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections;

namespace Sellsys.WpfClient.ViewModels.Dialogs
{
    /// <summary>
    /// 编辑收款信息对话框ViewModel
    /// </summary>
    public class EditPaymentInfoDialogViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private readonly ApiService _apiService;
        private FinanceOrderDetail _orderDetail;
        private decimal _receivedAmount;
        private DateTime? _paymentReceivedDate;
        private string _remarks = string.Empty;
        private bool _isLoading;

        // 验证相关
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        #region 属性

        /// <summary>
        /// 订单明细
        /// </summary>
        public FinanceOrderDetail OrderDetail
        {
            get => _orderDetail;
            set => SetProperty(ref _orderDetail, value);
        }

        /// <summary>
        /// 收款金额
        /// </summary>
        [Required(ErrorMessage = "收款金额不能为空")]
        [Range(0, double.MaxValue, ErrorMessage = "收款金额不能为负数")]
        public decimal ReceivedAmount
        {
            get => _receivedAmount;
            set
            {
                SetProperty(ref _receivedAmount, value);
                ValidateProperty(value);
                OnPropertyChanged(nameof(CanConfirm));
            }
        }

        /// <summary>
        /// 收款日期
        /// </summary>
        public DateTime? PaymentReceivedDate
        {
            get => _paymentReceivedDate;
            set => SetProperty(ref _paymentReceivedDate, value);
        }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500, ErrorMessage = "备注不能超过500个字符")]
        public string Remarks
        {
            get => _remarks;
            set
            {
                SetProperty(ref _remarks, value);
                ValidateProperty(value);
            }
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
        /// 是否可以确认
        /// </summary>
        public bool CanConfirm => !HasErrors && ReceivedAmount >= 0 && !IsLoading;

        #endregion

        #region INotifyDataErrorInfo 实现

        public bool HasErrors => _errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return _errors.SelectMany(x => x.Value);

            return _errors.ContainsKey(propertyName) ? _errors[propertyName] : Enumerable.Empty<string>();
        }

        #endregion

        #region 命令

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region 事件

        /// <summary>
        /// 关闭请求事件
        /// </summary>
        public event EventHandler<bool>? CloseRequested;

        #endregion

        #region 构造函数

        public EditPaymentInfoDialogViewModel(FinanceOrderDetail orderDetail)
        {
            _apiService = new ApiService();
            _orderDetail = orderDetail;
            
            // 初始化数据
            _receivedAmount = orderDetail.ReceivedAmount;
            _paymentReceivedDate = orderDetail.PaymentReceivedDate ?? DateTime.Now;
            _remarks = string.Empty;

            // 初始化命令
            ConfirmCommand = new AsyncRelayCommand(async p => await ConfirmAsync(), p => CanConfirm);
            CancelCommand = new RelayCommand(p => Cancel());
        }

        #endregion

        #region 方法

        /// <summary>
        /// 确认保存
        /// </summary>
        private async Task ConfirmAsync()
        {
            try
            {
                IsLoading = true;

                // 验证数据
                if (!ValidateAllProperties())
                {
                    return;
                }

                // 创建更新信息
                var updateInfo = new UpdatePaymentInfo
                {
                    OrderId = OrderDetail.OrderId,
                    ReceivedAmount = ReceivedAmount,
                    PaymentReceivedDate = PaymentReceivedDate,
                    Remarks = Remarks
                };

                // 调用API更新
                await _apiService.UpdatePaymentInfoAsync(updateInfo);

                // 更新本地数据
                OrderDetail.ReceivedAmount = ReceivedAmount;
                OrderDetail.PaymentReceivedDate = PaymentReceivedDate;
                OrderDetail.UnreceivedAmount = OrderDetail.TotalAmount - ReceivedAmount;
                OrderDetail.PaymentRatio = OrderDetail.TotalAmount == 0 ? 0 : (ReceivedAmount / OrderDetail.TotalAmount) * 100;

                // 关闭对话框并返回成功
                CloseRequested?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"保存收款信息失败: {ex.Message}", "错误", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 取消
        /// </summary>
        private void Cancel()
        {
            CloseRequested?.Invoke(this, false);
        }

        /// <summary>
        /// 验证属性
        /// </summary>
        private void ValidateProperty(object value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            var context = new ValidationContext(this) { MemberName = propertyName };
            var results = new List<ValidationResult>();

            ClearErrors(propertyName);

            if (!Validator.TryValidateProperty(value, context, results))
            {
                foreach (var result in results)
                {
                    AddError(propertyName, result.ErrorMessage ?? "验证失败");
                }
            }
        }

        /// <summary>
        /// 验证所有属性
        /// </summary>
        private bool ValidateAllProperties()
        {
            ValidateProperty(ReceivedAmount, nameof(ReceivedAmount));
            ValidateProperty(Remarks, nameof(Remarks));

            return !HasErrors;
        }

        /// <summary>
        /// 添加错误
        /// </summary>
        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();

            if (!_errors[propertyName].Contains(error))
            {
                _errors[propertyName].Add(error);
                OnErrorsChanged(propertyName);
                OnPropertyChanged(nameof(CanConfirm));
            }
        }

        /// <summary>
        /// 清除错误
        /// </summary>
        private void ClearErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
                OnPropertyChanged(nameof(CanConfirm));
            }
        }

        /// <summary>
        /// 触发错误变更事件
        /// </summary>
        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion
    }
}
