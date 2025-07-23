using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.Collections.ObjectModel;
using System.Linq;
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
        private bool _isLoading = false;

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
            set
            {
                if (SetProperty(ref _selectedProvince, value))
                {
                    // Update cities when province changes
                    UpdateCitiesForProvince(value);
                    // Clear selected city if it's not valid for the new province
                    if (!string.IsNullOrEmpty(SelectedCity) && !RegionDataService.IsCityInProvince(SelectedCity, value))
                    {
                        SelectedCity = string.Empty;
                    }
                }
            }
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

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
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
            IndustryTypes.Add("应急");
            IndustryTypes.Add("人社");
            IndustryTypes.Add("其它");

            // Initialize provinces using RegionDataService
            Provinces.Clear();
            var provinces = RegionDataService.GetProvinces();
            foreach (var province in provinces)
            {
                Provinces.Add(province);
            }

            // Initialize cities (empty initially, will be populated when province is selected)
            Cities.Clear();
        }

        private void UpdateCitiesForProvince(string province)
        {
            Cities.Clear();

            if (!string.IsNullOrEmpty(province))
            {
                var cities = RegionDataService.GetCitiesByProvince(province);
                foreach (var city in cities)
                {
                    Cities.Add(city);
                }
            }
        }

        private void LoadCustomerData()
        {
            if (_originalCustomer == null) return;

            // Load customer data into form
            CustomerName = _originalCustomer.Name;
            SelectedIndustryType = _originalCustomer.IndustryTypes ?? string.Empty;

            // Load province first, which will trigger city update
            SelectedProvince = _originalCustomer.Province ?? string.Empty;
            // Then load city after cities are populated
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
            if (IsLoading) return; // Prevent multiple simultaneous saves

            try
            {
                IsLoading = true;
                // Validate required fields
                if (string.IsNullOrWhiteSpace(CustomerName))
                {
                    MessageBox.Show("请输入客户名称", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate at least one contact
                var validContacts = Contacts.Where(c => !string.IsNullOrWhiteSpace(c.Name)).ToList();
                if (validContacts.Count == 0)
                {
                    MessageBox.Show("请至少添加一位联系人", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate contact phone numbers
                foreach (var contact in validContacts)
                {
                    if (!string.IsNullOrWhiteSpace(contact.Phone) && contact.Phone.Length < 11)
                    {
                        MessageBox.Show($"联系人 '{contact.Name}' 的电话号码格式不正确", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Check API availability before proceeding
                if (!await _apiService.IsApiAvailableAsync())
                {
                    MessageBox.Show("无法连接到服务器，请检查网络连接后重试", "网络错误", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                // Create DTO for API call
                var customerDto = new CustomerUpsertDto
                {
                    Name = CustomerName,
                    Province = SelectedProvince,
                    City = SelectedCity,
                    Address = DetailedAddress,
                    Remarks = CustomerRemarks,
                    IndustryTypes = SelectedIndustryType,
                    SalesPersonId = customer.SalesPersonId,
                    SupportPersonId = customer.SupportPersonId,
                    Contacts = Contacts.Where(c => !string.IsNullOrWhiteSpace(c.Name))
                        .Select(c => new ContactUpsertDto
                        {
                            Name = c.Name,
                            Phone = c.Phone,
                            IsPrimary = c.IsPrimary
                        }).ToList()
                };

                // Save customer via API
                if (_isEditMode && _originalCustomer != null)
                {
                    await _apiService.UpdateCustomerAsync(_originalCustomer.Id, customerDto);

                    // 发布客户更新事件
                    EventBus.Instance.Publish(new CustomerUpdatedEvent
                    {
                        CustomerId = _originalCustomer.Id,
                        CustomerName = CustomerName,
                        UpdateType = "Updated",
                        UpdatedAt = DateTime.Now
                    });
                }
                else
                {
                    var newCustomer = await _apiService.CreateCustomerAsync(customerDto);

                    // 发布客户创建事件
                    EventBus.Instance.Publish(new CustomerUpdatedEvent
                    {
                        CustomerId = newCustomer?.Id ?? 0,
                        CustomerName = CustomerName,
                        UpdateType = "Created",
                        UpdatedAt = DateTime.Now
                    });
                }

                CustomerSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                string errorMessage = "保存客户时发生错误";

                if (ex.Message.Contains("网络请求失败"))
                {
                    errorMessage = "网络连接失败，请检查网络连接后重试";
                }
                else if (ex.Message.Contains("请至少添加一位联系人"))
                {
                    errorMessage = "请至少添加一位联系人";
                }
                else if (ex.Message.Contains("timeout"))
                {
                    errorMessage = "请求超时，请稍后重试";
                }
                else
                {
                    errorMessage = $"保存失败: {ex.Message}";
                }

                MessageBox.Show(errorMessage, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
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
