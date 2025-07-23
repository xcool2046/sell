using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class EditContactRecordDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Customer _customer;
        private readonly SalesFollowUpLog _originalRecord;
        private ObservableCollection<Contact> _contacts;
        private Contact? _selectedContact;
        private string _contactPhone = string.Empty;
        private string _contactSummary = string.Empty;
        private DateTime? _nextFollowUpDate;
        
        // Customer Status
        private bool _isWaitingContact;
        private bool _isFollowingUp;
        private bool _isCompleted;
        
        // Customer Intention
        private bool _isIntentionUnknown;
        private bool _isIntentionHigh;
        private bool _isIntentionMedium;
        private bool _isIntentionLow;
        private bool _isIntentionNone;

        public EditContactRecordDialogViewModel(Customer customer, SalesFollowUpLog record)
        {
            _customer = customer ?? throw new ArgumentNullException(nameof(customer));
            _originalRecord = record ?? throw new ArgumentNullException(nameof(record));
            _apiService = new ApiService();
            _contacts = new ObservableCollection<Contact>();

            // Initialize commands
            CancelAppointmentCommand = new RelayCommand(p => CancelAppointment());
            CancelCommand = new RelayCommand(p => Cancel());
            SaveCommand = new RelayCommand(p => Save());

            // Load contacts and initialize data
            LoadContacts();
            InitializeFromRecord();
        }

        public string CustomerName => _customer.Name;

        public ObservableCollection<Contact> Contacts
        {
            get => _contacts;
            set => SetProperty(ref _contacts, value);
        }

        public Contact? SelectedContact
        {
            get => _selectedContact;
            set
            {
                if (SetProperty(ref _selectedContact, value))
                {
                    ContactPhone = value?.Phone ?? string.Empty;
                }
            }
        }

        public string ContactPhone
        {
            get => _contactPhone;
            set => SetProperty(ref _contactPhone, value);
        }

        public string ContactSummary
        {
            get => _contactSummary;
            set => SetProperty(ref _contactSummary, value);
        }

        public DateTime? NextFollowUpDate
        {
            get => _nextFollowUpDate;
            set => SetProperty(ref _nextFollowUpDate, value);
        }

        // Customer Status Properties
        public bool IsWaitingContact
        {
            get => _isWaitingContact;
            set => SetProperty(ref _isWaitingContact, value);
        }

        public bool IsFollowingUp
        {
            get => _isFollowingUp;
            set => SetProperty(ref _isFollowingUp, value);
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetProperty(ref _isCompleted, value);
        }

        // Customer Intention Properties
        public bool IsIntentionUnknown
        {
            get => _isIntentionUnknown;
            set => SetProperty(ref _isIntentionUnknown, value);
        }

        public bool IsIntentionHigh
        {
            get => _isIntentionHigh;
            set => SetProperty(ref _isIntentionHigh, value);
        }

        public bool IsIntentionMedium
        {
            get => _isIntentionMedium;
            set => SetProperty(ref _isIntentionMedium, value);
        }

        public bool IsIntentionLow
        {
            get => _isIntentionLow;
            set => SetProperty(ref _isIntentionLow, value);
        }

        public bool IsIntentionNone
        {
            get => _isIntentionNone;
            set => SetProperty(ref _isIntentionNone, value);
        }

        // Commands
        public ICommand CancelAppointmentCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SaveCommand { get; }

        // Events
        public event EventHandler? CloseRequested;
        public event EventHandler? SaveCompleted;

        private void LoadContacts()
        {
            try
            {
                Contacts.Clear();
                foreach (var contact in _customer.Contacts)
                {
                    Contacts.Add(contact);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载联系人失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeFromRecord()
        {
            try
            {
                // Set basic information
                ContactSummary = _originalRecord.Summary ?? string.Empty;
                NextFollowUpDate = _originalRecord.NextFollowUpDate;

                // Find and select the contact
                if (_originalRecord.ContactId.HasValue)
                {
                    SelectedContact = Contacts.FirstOrDefault(c => c.Id == _originalRecord.ContactId.Value);
                }

                // Set customer intention
                SetCustomerIntention(_originalRecord.CustomerIntention ?? "未知");

                // Set customer status (default to "跟进中" for existing records)
                IsFollowingUp = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化记录数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetCustomerIntention(string intention)
        {
            // Reset all intention flags
            IsIntentionUnknown = false;
            IsIntentionHigh = false;
            IsIntentionMedium = false;
            IsIntentionLow = false;
            IsIntentionNone = false;

            // Set the appropriate flag
            switch (intention)
            {
                case "高":
                    IsIntentionHigh = true;
                    break;
                case "中":
                    IsIntentionMedium = true;
                    break;
                case "低":
                    IsIntentionLow = true;
                    break;
                case "无":
                    IsIntentionNone = true;
                    break;
                default:
                    IsIntentionUnknown = true;
                    break;
            }
        }

        private void CancelAppointment()
        {
            NextFollowUpDate = null;
        }

        private async void Save()
        {
            try
            {
                // Validate required fields
                if (SelectedContact == null)
                {
                    MessageBox.Show("请选择联系人", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(ContactSummary))
                {
                    MessageBox.Show("请输入联系情况", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get selected status and intention
                string customerStatus = GetSelectedCustomerStatus();
                string customerIntention = GetSelectedCustomerIntention();

                // Create update DTO
                var updateDto = new SalesFollowUpLogUpsertDto
                {
                    CustomerId = _customer.Id,
                    ContactId = SelectedContact.Id,
                    Summary = ContactSummary,
                    CustomerIntention = customerIntention,
                    NextFollowUpDate = NextFollowUpDate
                };

                // Call API to update the contact record
                await _apiService.UpdateSalesFollowUpLogAsync(_originalRecord.Id, updateDto);

                // Update the original record for UI consistency
                _originalRecord.ContactId = SelectedContact.Id;
                _originalRecord.ContactName = SelectedContact.Name;
                _originalRecord.Summary = ContactSummary;
                _originalRecord.CustomerIntention = customerIntention;
                _originalRecord.NextFollowUpDate = NextFollowUpDate;

                SaveCompleted?.Invoke(this, EventArgs.Empty);
                Cancel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新联系记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetSelectedCustomerStatus()
        {
            if (IsWaitingContact) return "待联系";
            if (IsFollowingUp) return "跟进中";
            if (IsCompleted) return "已成交";
            return "跟进中";
        }

        private string GetSelectedCustomerIntention()
        {
            if (IsIntentionHigh) return "高";
            if (IsIntentionMedium) return "中";
            if (IsIntentionLow) return "低";
            if (IsIntentionNone) return "无";
            return "未知";
        }

        private void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
