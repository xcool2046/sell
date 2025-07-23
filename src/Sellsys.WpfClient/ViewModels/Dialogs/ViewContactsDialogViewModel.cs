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

        // Events
        public event EventHandler? CloseRequested;

        public ViewContactsDialogViewModel(Customer customer)
        {
            _customer = customer;
            _contacts = new ObservableCollection<ContactDisplayModel>();

            // Initialize commands
            CloseCommand = new RelayCommand(p => Close());

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

            // No mock contacts - display actual data only
        }

        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }


    }

    public class ContactDisplayModel : ViewModelBase
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
