using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class AddEditCustomerViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Customer? _originalCustomer;
        private bool _isEditMode;
        private bool _isSaving;

        // Customer properties
        private string _name = string.Empty;
        private string? _province;
        private string? _city;
        private string? _address;
        private string? _remarks;
        private string? _industryTypes;
        private int? _salesPersonId;
        private int? _supportPersonId;

        // Collections
        private ObservableCollection<Contact> _contacts;

        public event EventHandler? CustomerSaved;

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

        [Required(ErrorMessage = "客户名称不能为空")]
        [StringLength(255, ErrorMessage = "客户名称长度不能超过255个字符")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string? Province
        {
            get => _province;
            set => SetProperty(ref _province, value);
        }

        public string? City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }

        public string? Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public string? Remarks
        {
            get => _remarks;
            set => SetProperty(ref _remarks, value);
        }

        public string? IndustryTypes
        {
            get => _industryTypes;
            set => SetProperty(ref _industryTypes, value);
        }

        public int? SalesPersonId
        {
            get => _salesPersonId;
            set => SetProperty(ref _salesPersonId, value);
        }

        public int? SupportPersonId
        {
            get => _supportPersonId;
            set => SetProperty(ref _supportPersonId, value);
        }

        public ObservableCollection<Contact> Contacts
        {
            get => _contacts;
            set => SetProperty(ref _contacts, value);
        }

        // Commands
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand AddContactCommand { get; private set; }
        public ICommand RemoveContactCommand { get; private set; }

        public string Title => IsEditMode ? "编辑客户" : "添加客户";
        public string SaveButtonText => IsEditMode ? "更新" : "保存";

        // Constructor for adding new customer
        public AddEditCustomerViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _originalCustomer = null;
            _isEditMode = false;
            _contacts = new ObservableCollection<Contact>();

            InitializeCommands();
            AddDefaultContact();
        }

        // Constructor for editing existing customer
        public AddEditCustomerViewModel(ApiService apiService, Customer customer)
        {
            _apiService = apiService;
            _originalCustomer = customer;
            _isEditMode = true;

            // Copy customer data
            _name = customer.Name;
            _province = customer.Province;
            _city = customer.City;
            _address = customer.Address;
            _remarks = customer.Remarks;
            _industryTypes = customer.IndustryTypes;
            _salesPersonId = customer.SalesPersonId;
            _supportPersonId = customer.SupportPersonId;
            _contacts = new ObservableCollection<Contact>(customer.Contacts);

            InitializeCommands();

            // Ensure at least one contact
            if (!_contacts.Any())
            {
                AddDefaultContact();
            }
        }

        private void InitializeCommands()
        {
            SaveCommand = new RelayCommand(async p => await SaveCustomerAsync(), p => CanSave());
            CancelCommand = new RelayCommand(p => Cancel());
            AddContactCommand = new RelayCommand(p => AddContact());
            RemoveContactCommand = new RelayCommand(p => RemoveContact(p as Contact), p => p is Contact && Contacts.Count > 1);
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name) && 
                   Contacts.Any() && 
                   Contacts.All(c => !string.IsNullOrWhiteSpace(c.Name)) &&
                   !IsSaving;
        }

        private async Task SaveCustomerAsync()
        {
            if (!ValidateInput()) return;

            try
            {
                IsSaving = true;

                var customerDto = new CustomerUpsertDto
                {
                    Name = Name.Trim(),
                    Province = Province?.Trim(),
                    City = City?.Trim(),
                    Address = Address?.Trim(),
                    Remarks = Remarks?.Trim(),
                    IndustryTypes = IndustryTypes?.Trim(),
                    SalesPersonId = SalesPersonId,
                    SupportPersonId = SupportPersonId,
                    Contacts = Contacts.Select(c => new ContactUpsertDto
                    {
                        Id = IsEditMode ? c.Id : null,
                        Name = c.Name.Trim(),
                        Phone = c.Phone?.Trim(),
                        IsPrimary = c.IsPrimary
                    }).ToList()
                };

                if (IsEditMode && _originalCustomer != null)
                {
                    await _apiService.UpdateCustomerAsync(_originalCustomer.Id, customerDto);
                    MessageBox.Show("客户信息更新成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _apiService.CreateCustomerAsync(customerDto);
                    MessageBox.Show("客户添加成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CustomerSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存客户失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("请输入客户名称", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!Contacts.Any())
            {
                MessageBox.Show("请至少添加一位联系人", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Contacts.Any(c => string.IsNullOrWhiteSpace(c.Name)))
            {
                MessageBox.Show("所有联系人都必须填写姓名", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Ensure at least one primary contact
            if (!Contacts.Any(c => c.IsPrimary))
            {
                Contacts.First().IsPrimary = true;
            }

            // Ensure only one primary contact
            var primaryContacts = Contacts.Where(c => c.IsPrimary).ToList();
            if (primaryContacts.Count > 1)
            {
                for (int i = 1; i < primaryContacts.Count; i++)
                {
                    primaryContacts[i].IsPrimary = false;
                }
            }

            return true;
        }

        private void Cancel()
        {
            // TODO: Close dialog
        }

        private void AddContact()
        {
            var newContact = new Contact
            {
                Name = string.Empty,
                Phone = string.Empty,
                IsPrimary = !Contacts.Any()
            };
            Contacts.Add(newContact);
        }

        private void AddDefaultContact()
        {
            AddContact();
        }

        private void RemoveContact(Contact? contact)
        {
            if (contact != null && Contacts.Count > 1)
            {
                // If removing primary contact, make the first remaining contact primary
                bool wasPrimary = contact.IsPrimary;
                Contacts.Remove(contact);
                
                if (wasPrimary && Contacts.Any())
                {
                    Contacts.First().IsPrimary = true;
                }
            }
        }
    }
}
