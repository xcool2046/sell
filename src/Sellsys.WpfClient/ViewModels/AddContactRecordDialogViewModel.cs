using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class AddContactRecordDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Customer _customer;
        private ObservableCollection<Contact> _contacts;
        private Contact? _selectedContact;
        private string _contactPhone = string.Empty;
        private string _contactSummary = string.Empty;
        private DateTime? _nextFollowUpDate;
        
        // Customer Status
        private bool _isWaitingContact = true;
        private bool _isFollowingUp;
        private bool _isCompleted;
        
        // Customer Intention
        private bool _isIntentionHigh = true; // 默认选择"高"
        private bool _isIntentionMedium;
        private bool _isIntentionLow;

        public AddContactRecordDialogViewModel(Customer customer)
        {
            _customer = customer ?? throw new ArgumentNullException(nameof(customer));
            _apiService = new ApiService();
            _contacts = new ObservableCollection<Contact>();

            // Initialize default values
            _nextFollowUpDate = DateTime.Today.AddDays(1); // 默认设置为明天

            // Initialize commands
            CancelAppointmentCommand = new RelayCommand(p => CancelAppointment());
            CloseCommand = new RelayCommand(p => Close());
            SaveCommand = new RelayCommand(p => Save());

            // Load contacts
            LoadContacts();
        }

        public string CustomerName => _customer.Name;

        // 显示当前时间
        public string CurrentDateTime => DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        // 销售人员名称
        public string SalesPersonName => _customer.SalesPersonName ?? "未分配";

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

        // Commands
        public ICommand CancelAppointmentCommand { get; }
        public ICommand CloseCommand { get; }
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

                // Select primary contact by default
                SelectedContact = _customer.PrimaryContact;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载联系人失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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

                // Create contact record DTO for API call
                var contactRecordDto = new SalesFollowUpLogUpsertDto
                {
                    CustomerId = _customer.Id,
                    ContactId = SelectedContact?.Id,
                    Summary = ContactSummary,
                    CustomerIntention = customerIntention,
                    NextFollowUpDate = NextFollowUpDate,
                    SalesPersonId = null // TODO: Get current user ID
                };

                // Call API to save the contact record
                await _apiService.CreateSalesFollowUpLogAsync(contactRecordDto);

                SaveCompleted?.Invoke(this, EventArgs.Empty);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存联系记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetSelectedCustomerStatus()
        {
            if (IsWaitingContact) return "待联系";
            if (IsFollowingUp) return "跟进中";
            if (IsCompleted) return "已成交";
            return "待联系";
        }

        private string GetSelectedCustomerIntention()
        {
            if (IsIntentionHigh) return "高";
            if (IsIntentionMedium) return "中";
            if (IsIntentionLow) return "低";
            return "高"; // 默认返回"高"
        }

        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
