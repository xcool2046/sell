using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class CustomerDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Customer? _originalCustomer;
        private readonly bool _isEditMode;
        
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

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddContactCommand { get; }
        public ICommand RemoveContactCommand { get; }

        public event EventHandler? CustomerSaved;
        public event EventHandler? Cancelled;

        // Constructor for adding new customer
        public CustomerDialogViewModel(ApiService apiService) : this(apiService, null)
        {
        }

        // Constructor for editing existing customer
        public CustomerDialogViewModel(ApiService apiService, Customer? customer)
        {
            _apiService = apiService;
            _originalCustomer = customer;
            _isEditMode = customer != null;

            // Initialize collections
            _industryTypes = new ObservableCollection<string>();
            _provinces = new ObservableCollection<string>();
            _cities = new ObservableCollection<string>();
            _contacts = new ObservableCollection<ContactModel>();

            // Initialize commands
            SaveCommand = new AsyncRelayCommand(async p => await SaveCustomerAsync());
            CancelCommand = new RelayCommand(p => Cancel());
            AddContactCommand = new RelayCommand(p => AddContact());
            RemoveContactCommand = new RelayCommand(p => RemoveContact(p as ContactModel));

            // Initialize data
            InitializeData();
            
            if (_isEditMode)
            {
                LoadCustomerData();
            }
            else
            {
                // Add initial contact for new customer
                AddContact();
            }
        }

        private void InitializeData()
        {
            // Initialize industry types
            IndustryTypes.Add("制造业");
            IndustryTypes.Add("服务业");
            IndustryTypes.Add("科技");
            IndustryTypes.Add("金融");

            // Initialize provinces
            Provinces.Add("四川");
            Provinces.Add("广东");
            Provinces.Add("北京");
            Provinces.Add("上海");
            Provinces.Add("江苏");
            Provinces.Add("浙江");

            // Initialize cities
            Cities.Add("成都");
            Cities.Add("广州");
            Cities.Add("深圳");
            Cities.Add("北京");
            Cities.Add("上海");
            Cities.Add("南京");
            Cities.Add("杭州");
        }

        private void LoadCustomerData()
        {
            if (_originalCustomer == null) return;

            // Load customer data into form
            CustomerName = _originalCustomer.Name;
            SelectedIndustryType = _originalCustomer.IndustryTypes ?? string.Empty;
            SelectedProvince = _originalCustomer.Province ?? string.Empty;
            SelectedCity = _originalCustomer.City ?? string.Empty;
            DetailedAddress = _originalCustomer.Address ?? string.Empty;
            CustomerRemarks = _originalCustomer.Remarks ?? string.Empty;

            // Load contacts
            Contacts.Clear();
            if (_originalCustomer.Contacts != null)
            {
                foreach (var contact in _originalCustomer.Contacts)
                {
                    Contacts.Add(new ContactModel
                    {
                        Name = contact.Name,
                        Phone = contact.Phone,
                        IsPrimary = contact.IsPrimary
                    });
                }
            }

            // Ensure at least one contact
            if (Contacts.Count == 0)
            {
                AddContact();
            }
        }

        private void AddContact()
        {
            Contacts.Add(new ContactModel());
        }

        private void RemoveContact(ContactModel? contact)
        {
            if (contact != null && Contacts.Count > 1)
            {
                Contacts.Remove(contact);
            }
        }

        private async Task SaveCustomerAsync()
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(CustomerName))
                {
                    MessageBox.Show("请输入客户名称", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Customer customer;
                if (_isEditMode && _originalCustomer != null)
                {
                    // Update existing customer
                    customer = _originalCustomer;
                    customer.Name = CustomerName;
                    customer.IndustryTypes = SelectedIndustryType;
                    customer.Province = SelectedProvince;
                    customer.City = SelectedCity;
                    customer.Address = DetailedAddress;
                    customer.Remarks = CustomerRemarks;

                    // Update contacts
                    customer.Contacts.Clear();
                }
                else
                {
                    // Create new customer
                    customer = new Customer
                    {
                        Name = CustomerName,
                        IndustryTypes = SelectedIndustryType,
                        Province = SelectedProvince,
                        City = SelectedCity,
                        Address = DetailedAddress,
                        Remarks = CustomerRemarks
                    };
                }

                // Add contacts
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
                // if (_isEditMode)
                //     await _apiService.UpdateCustomerAsync(customer);
                // else
                //     await _apiService.CreateCustomerAsync(customer);

                // Simulate a brief delay to show loading state
                await Task.Delay(500);

                string action = _isEditMode ? "更新" : "添加";
                MessageBox.Show($"客户 '{CustomerName}' {action}成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                CustomerSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存客户时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
