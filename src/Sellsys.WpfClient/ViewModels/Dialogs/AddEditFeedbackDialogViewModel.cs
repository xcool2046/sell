using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels.Dialogs
{
    public class AddEditFeedbackDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Customer _customer;
        private readonly AfterSalesRecord? _originalRecord;
        private readonly List<Employee> _employees;
        private bool _isEditMode;
        private bool _isSaving;

        // Form fields
        private string? _customerFeedback;
        private string? _ourReply;
        private string _status = "待处理";
        private DateTime? _processedDate;
        private int? _supportPersonId;
        private int? _contactId;

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        // 客户信息（只读）
        public string CustomerUnit => _customer.Name;
        public string CustomerName => _customer.Contacts?.FirstOrDefault(c => c.IsPrimary)?.Name ?? "未设置";
        public string CustomerPhone => _customer.Contacts?.FirstOrDefault(c => c.IsPrimary)?.Phone ?? "未设置";

        // 表单字段
        [Required(ErrorMessage = "客户反馈不能为空")]
        [StringLength(2000, ErrorMessage = "客户反馈内容不能超过2000个字符")]
        public string? CustomerFeedback
        {
            get => _customerFeedback;
            set => SetProperty(ref _customerFeedback, value);
        }

        [StringLength(2000, ErrorMessage = "回复内容不能超过2000个字符")]
        public string? OurReply
        {
            get => _ourReply;
            set => SetProperty(ref _ourReply, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public DateTime? ProcessedDate
        {
            get => _processedDate;
            set => SetProperty(ref _processedDate, value);
        }

        public int? SupportPersonId
        {
            get => _supportPersonId;
            set => SetProperty(ref _supportPersonId, value);
        }

        public int? ContactId
        {
            get => _contactId;
            set => SetProperty(ref _contactId, value);
        }

        // 选项列表
        public List<string> StatusOptions { get; } = new List<string>
        {
            "待处理", "处理中", "处理完成"
        };

        public List<Employee> Employees => _employees;
        public List<Contact> CustomerContacts => _customer.Contacts?.ToList() ?? new List<Contact>();

        // Commands
        public ICommand SaveCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        public string Title => IsEditMode ? "编辑反馈" : "添加反馈";
        public string SaveButtonText => IsEditMode ? "更新" : "保存";

        public event EventHandler? RequestClose;
        public event EventHandler? RecordSaved;

        // Constructor for adding new record
        public AddEditFeedbackDialogViewModel(ApiService apiService, Customer customer, List<Employee> employees)
        {
            _apiService = apiService;
            _customer = customer;
            _employees = employees;
            _originalRecord = null;
            _isEditMode = false;

            // 设置默认联系人
            var primaryContact = _customer.Contacts?.FirstOrDefault(c => c.IsPrimary);
            if (primaryContact != null)
            {
                _contactId = primaryContact.Id;
            }

            InitializeCommands();
        }

        // Constructor for editing existing record
        public AddEditFeedbackDialogViewModel(ApiService apiService, Customer customer, List<Employee> employees, AfterSalesRecord record)
        {
            _apiService = apiService;
            _customer = customer;
            _employees = employees;
            _originalRecord = record;
            _isEditMode = true;

            // Copy record data
            _customerFeedback = record.CustomerFeedback;
            _ourReply = record.OurReply;
            _status = record.Status;
            _processedDate = record.ProcessedDate;
            _supportPersonId = record.SupportPersonId;
            _contactId = record.ContactId;

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            SaveCommand = new AsyncRelayCommand(async p => await SaveRecordAsync(), p => CanSave());
            CloseCommand = new RelayCommand(p => Close());

            // 监听属性变化以更新命令状态
            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(CustomerFeedback) || e.PropertyName == nameof(IsSaving))
                {
                    // 通知命令状态可能已更改
                    CommandManager.InvalidateRequerySuggested();
                }
            };
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(CustomerFeedback) && !IsSaving;
        }

        private async Task SaveRecordAsync()
        {
            if (!ValidateInput()) return;

            try
            {
                IsSaving = true;

                var recordDto = new AfterSalesRecordUpsertDto
                {
                    CustomerId = _customer.Id,
                    ContactId = ContactId,
                    CustomerFeedback = CustomerFeedback?.Trim(),
                    OurReply = OurReply?.Trim(),
                    Status = Status,
                    ProcessedDate = ProcessedDate,
                    SupportPersonId = SupportPersonId
                };

                if (IsEditMode && _originalRecord != null)
                {
                    await _apiService.UpdateAfterSalesRecordAsync(_originalRecord.Id, recordDto);
                }
                else
                {
                    await _apiService.CreateAfterSalesRecordAsync(recordDto);
                }

                RecordSaved?.Invoke(this, EventArgs.Empty);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存售后记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(CustomerFeedback))
            {
                MessageBox.Show("请输入客户反馈内容", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CustomerFeedback.Length > 2000)
            {
                MessageBox.Show("客户反馈内容不能超过2000个字符", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!string.IsNullOrEmpty(OurReply) && OurReply.Length > 2000)
            {
                MessageBox.Show("回复内容不能超过2000个字符", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void Close()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
