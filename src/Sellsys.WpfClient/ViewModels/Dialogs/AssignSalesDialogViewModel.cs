using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class AssignSalesDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Customer _customer;
        
        private string _departmentName = "销售部";
        private ObservableCollection<GroupModel> _groups;
        private ObservableCollection<SalesPersonModel> _salesPersons;
        private GroupModel? _selectedGroup;
        private SalesPersonModel? _selectedSalesPerson;
        private bool _isLoading = false;

        public string DepartmentName
        {
            get => _departmentName;
            set => SetProperty(ref _departmentName, value);
        }

        public ObservableCollection<GroupModel> Groups
        {
            get => _groups;
            set => SetProperty(ref _groups, value);
        }

        public ObservableCollection<SalesPersonModel> SalesPersons
        {
            get => _salesPersons;
            set => SetProperty(ref _salesPersons, value);
        }

        public GroupModel? SelectedGroup
        {
            get => _selectedGroup;
            set 
            { 
                SetProperty(ref _selectedGroup, value);
                LoadSalesPersonsForGroup();
            }
        }

        public SalesPersonModel? SelectedSalesPerson
        {
            get => _selectedSalesPerson;
            set => SetProperty(ref _selectedSalesPerson, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Commands
        public ICommand AssignCommand { get; }
        public ICommand CancelCommand { get; }

        // Events
        public event EventHandler? AssignmentCompleted;
        public event EventHandler? Cancelled;

        public AssignSalesDialogViewModel(ApiService apiService, Customer customer)
        {
            _apiService = apiService;
            _customer = customer;
            
            // Initialize collections
            _groups = new ObservableCollection<GroupModel>();
            _salesPersons = new ObservableCollection<SalesPersonModel>();

            // Initialize commands
            AssignCommand = new AsyncRelayCommand(async p => await AssignSalesAsync());
            CancelCommand = new RelayCommand(p => Cancel());

            // Initialize data
            InitializeData();
        }

        private void InitializeData()
        {
            // Initialize groups
            Groups.Add(new GroupModel { Id = 1, Name = "销售一组" });
            Groups.Add(new GroupModel { Id = 2, Name = "销售二组" });
            Groups.Add(new GroupModel { Id = 3, Name = "销售三组" });

            // Set default selection
            SelectedGroup = Groups.FirstOrDefault();
        }

        private void LoadSalesPersonsForGroup()
        {
            SalesPersons.Clear();
            
            if (SelectedGroup == null) return;

            // Mock data based on selected group
            switch (SelectedGroup.Id)
            {
                case 1:
                    SalesPersons.Add(new SalesPersonModel { Id = 1, Name = "张飞" });
                    SalesPersons.Add(new SalesPersonModel { Id = 2, Name = "李逵" });
                    break;
                case 2:
                    SalesPersons.Add(new SalesPersonModel { Id = 3, Name = "陈小二" });
                    SalesPersons.Add(new SalesPersonModel { Id = 4, Name = "王五" });
                    break;
                case 3:
                    SalesPersons.Add(new SalesPersonModel { Id = 5, Name = "赵六" });
                    SalesPersons.Add(new SalesPersonModel { Id = 6, Name = "孙七" });
                    break;
            }

            SelectedSalesPerson = SalesPersons.FirstOrDefault();
        }

        private async Task AssignSalesAsync()
        {
            if (IsLoading) return; // Prevent multiple simultaneous assignments

            try
            {
                IsLoading = true;
                if (SelectedSalesPerson == null)
                {
                    MessageBox.Show("请选择销售人员", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Call API to assign sales person to customer
                await _apiService.AssignSalesPersonAsync(_customer.Id, SelectedSalesPerson.Id);

                // Update customer's sales person
                _customer.SalesPersonId = SelectedSalesPerson.Id;
                _customer.SalesPersonName = SelectedSalesPerson.Name;

                // Publish assignment event for other modules
                EventBus.Instance.Publish(new CustomerAssignedEvent
                {
                    CustomerId = _customer.Id,
                    CustomerName = _customer.Name,
                    SalesPersonId = SelectedSalesPerson.Id,
                    SalesPersonName = SelectedSalesPerson.Name,
                    AssignedAt = DateTime.Now,
                    AssignmentType = "Sales"
                });

                MessageBox.Show($"已成功将客户 '{_customer.Name}' 分配给销售 '{SelectedSalesPerson.Name}'",
                    "分配成功", MessageBoxButton.OK, MessageBoxImage.Information);

                AssignmentCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"分配销售失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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

    public class GroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class SalesPersonModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
