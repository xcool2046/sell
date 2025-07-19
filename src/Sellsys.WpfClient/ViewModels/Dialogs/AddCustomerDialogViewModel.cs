using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class AddCustomerDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        
        // Basic properties
        private string _customerName = string.Empty;
        private string _selectedIndustryType = string.Empty;
        private string _selectedProvince = string.Empty;
        private string _selectedCity = string.Empty;
        private string _detailedAddress = string.Empty;
        private string _customerRemarks = string.Empty;
        
        // Collections
        private ObservableCollection<string> _industryTypes;
        private ObservableCollection<string> _provinces;
        private ObservableCollection<string> _cities;
        private ObservableCollection<ContactModel> _contacts;

        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
        }

        public string SelectedIndustryType
        {
            get => _selectedIndustryType;
            set => SetProperty(ref _selectedIndustryType, value);
        }

        public string SelectedProvince
        {
            get => _selectedProvince;
            set => SetProperty(ref _selectedProvince, value);
        }

        public string SelectedCity
        {
            get => _selectedCity;
            set => SetProperty(ref _selectedCity, value);
        }

        public string DetailedAddress
        {
            get => _detailedAddress;
            set => SetProperty(ref _detailedAddress, value);
        }

        public string CustomerRemarks
        {
            get => _customerRemarks;
            set => SetProperty(ref _customerRemarks, value);
        }

        public ObservableCollection<string> IndustryTypes
        {
            get => _industryTypes;
            set => SetProperty(ref _industryTypes, value);
        }

        public ObservableCollection<string> Provinces
        {
            get => _provinces;
            set => SetProperty(ref _provinces, value);
        }

        public ObservableCollection<string> Cities
        {
            get => _cities;
            set => SetProperty(ref _cities, value);
        }

        public ObservableCollection<ContactModel> Contacts
        {
            get => _contacts;
            set => SetProperty(ref _contacts, value);
        }

        // Commands
        public ICommand AddContactCommand { get; }
        public ICommand RemoveContactCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Events
        public event EventHandler? CustomerSaved;
        public event EventHandler? Cancelled;

        public AddCustomerDialogViewModel(ApiService apiService)
        {
            _apiService = apiService;
            
            // Initialize collections
            _industryTypes = new ObservableCollection<string>();
            _provinces = new ObservableCollection<string>();
            _cities = new ObservableCollection<string>();
            _contacts = new ObservableCollection<ContactModel>();

            // Initialize commands
            AddContactCommand = new RelayCommand(p => AddContact());
            RemoveContactCommand = new RelayCommand(p => RemoveContact(p as ContactModel));
            SaveCommand = new AsyncRelayCommand(async p => await SaveCustomerAsync());
            CancelCommand = new RelayCommand(p => Cancel());

            // Initialize data
            InitializeData();
        }

        private void InitializeData()
        {
            // Initialize industry types
            IndustryTypes.Add("教育");
            IndustryTypes.Add("医疗");
            IndustryTypes.Add("制造业");
            IndustryTypes.Add("服务业");
            IndustryTypes.Add("科技");
            IndustryTypes.Add("金融");

            // Initialize provinces
            Provinces.Add("广东");
            Provinces.Add("北京");
            Provinces.Add("上海");
            Provinces.Add("江苏");
            Provinces.Add("浙江");

            // Initialize cities
            Cities.Add("广州");
            Cities.Add("深圳");
            Cities.Add("成都");
            Cities.Add("四川");

            // Add initial contact
            AddContact();
        }

        private void AddContact()
        {
            Contacts.Add(new ContactModel
            {
                Name = string.Empty,
                Phone = string.Empty,
                IsPrimary = Contacts.Count == 0 // First contact is primary by default
            });
        }

        private void RemoveContact(ContactModel? contact)
        {
            if (contact != null && Contacts.Count > 1) // Keep at least one contact
            {
                Contacts.Remove(contact);
            }
        }

        private async Task SaveCustomerAsync()
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(CustomerName))
                {
                    MessageBox.Show("请输入客户单位名称", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Contacts.Count == 0 || Contacts.All(c => string.IsNullOrWhiteSpace(c.Name)))
                {
                    MessageBox.Show("请至少添加一位联系人", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create customer object
                var customer = new Customer
                {
                    Name = CustomerName,
                    IndustryTypes = SelectedIndustryType,
                    Province = SelectedProvince,
                    City = SelectedCity,
                    Address = DetailedAddress,
                    Remarks = CustomerRemarks
                };

                // Convert ContactModel to Contact
                foreach (var contactModel in Contacts.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
                {
                    customer.Contacts.Add(new Contact
                    {
                        Name = contactModel.Name,
                        Phone = contactModel.Phone,
                        IsPrimary = contactModel.IsPrimary
                    });
                }

                // Save customer - for now just show success message
                // TODO: Implement actual API call when backend is ready
                // await _apiService.CreateCustomerAsync(customer);

                // Simulate a brief delay to show loading state
                await Task.Delay(500);

                MessageBox.Show($"客户 '{CustomerName}' 添加成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                CustomerSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存客户失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ContactModel : ViewModelBase
    {
        private string _name = string.Empty;
        private string _phone = string.Empty;
        private bool _isPrimary;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public bool IsPrimary
        {
            get => _isPrimary;
            set => SetProperty(ref _isPrimary, value);
        }
    }
}
