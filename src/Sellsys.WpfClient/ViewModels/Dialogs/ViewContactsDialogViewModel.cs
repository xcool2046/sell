using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class ViewContactsDialogViewModel : ViewModelBase
    {
        private readonly Customer _customer;
        
        private string _customerName = string.Empty;
        private string _detailedAddress = string.Empty;
        private string _customerRemarks = string.Empty;
        private ObservableCollection<ContactDisplayModel> _contacts;

        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
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

        public ObservableCollection<ContactDisplayModel> Contacts
        {
            get => _contacts;
            set => SetProperty(ref _contacts, value);
        }

        // Commands
        public ICommand CloseCommand { get; }
        public ICommand SaveCommand { get; }

        // Events
        public event EventHandler? CloseRequested;
        public event EventHandler? ContactsUpdated;

        public ViewContactsDialogViewModel(Customer customer)
        {
            _customer = customer;
            _contacts = new ObservableCollection<ContactDisplayModel>();

            // Initialize commands
            CloseCommand = new RelayCommand(p => Close());
            SaveCommand = new RelayCommand(p => SaveChanges());

            // Initialize data
            InitializeData();
        }

        private void InitializeData()
        {
            CustomerName = _customer.Name;
            DetailedAddress = $"{_customer.Province ?? ""} {_customer.City ?? ""} {_customer.Address ?? ""}".Trim();
            CustomerRemarks = _customer.Remarks ?? string.Empty;

            // Load contacts
            Contacts.Clear();
            if (_customer.Contacts != null)
            {
                foreach (var contact in _customer.Contacts)
                {
                    Contacts.Add(new ContactDisplayModel
                    {
                        Name = contact.Name,
                        Phone = contact.Phone,
                        IsPrimary = contact.IsPrimary
                    });
                }
            }

            // Add mock contacts if none exist
            if (Contacts.Count == 0)
            {
                Contacts.Add(new ContactDisplayModel
                {
                    Name = "刘蛇立",
                    Phone = "15862184966",
                    IsPrimary = true,
                    IsSupport = false
                });
                Contacts.Add(new ContactDisplayModel
                {
                    Name = "李逵",
                    Phone = "13956774892",
                    IsPrimary = false,
                    IsSupport = true
                });
            }
        }

        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SaveChanges()
        {
            try
            {
                // TODO: 实际保存联系人的支持人/关键人标识到数据库
                // 这里可以调用API来更新联系人信息

                MessageBox.Show("联系人信息已更新", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                ContactsUpdated?.Invoke(this, EventArgs.Empty);
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class ContactDisplayModel : ViewModelBase
    {
        private string _name = string.Empty;
        private string _phone = string.Empty;
        private bool _isPrimary;
        private bool _isSupport;

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

        public bool IsSupport
        {
            get => _isSupport;
            set => SetProperty(ref _isSupport, value);
        }
    }
}
