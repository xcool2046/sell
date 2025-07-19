using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Sellsys.WpfClient.ViewModels
{
    public class AssignSupportDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Customer _customer;
        
        private string _departmentName = "客服部";
        private ObservableCollection<GroupModel> _groups;
        private ObservableCollection<SupportPersonModel> _supportPersons;
        private GroupModel? _selectedGroup;
        private SupportPersonModel? _selectedSupportPerson;

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

        public ObservableCollection<SupportPersonModel> SupportPersons
        {
            get => _supportPersons;
            set => SetProperty(ref _supportPersons, value);
        }

        public GroupModel? SelectedGroup
        {
            get => _selectedGroup;
            set 
            { 
                SetProperty(ref _selectedGroup, value);
                LoadSupportPersonsForGroup();
            }
        }

        public SupportPersonModel? SelectedSupportPerson
        {
            get => _selectedSupportPerson;
            set => SetProperty(ref _selectedSupportPerson, value);
        }

        // Commands
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Events
        public event EventHandler? AssignmentCompleted;
        public event EventHandler? Cancelled;

        public AssignSupportDialogViewModel(ApiService apiService, Customer customer)
        {
            _apiService = apiService;
            _customer = customer;
            
            // Initialize collections
            _groups = new ObservableCollection<GroupModel>();
            _supportPersons = new ObservableCollection<SupportPersonModel>();

            // Initialize commands
            SaveCommand = new AsyncRelayCommand(async p => await AssignSupportAsync());
            CancelCommand = new RelayCommand(p => Cancel());

            // Initialize data
            InitializeData();
        }

        private void InitializeData()
        {
            // Initialize groups
            Groups.Add(new GroupModel { Id = 1, Name = "客服一组" });
            Groups.Add(new GroupModel { Id = 2, Name = "客服二组" });
            Groups.Add(new GroupModel { Id = 3, Name = "客服三组" });

            // Set default selection
            SelectedGroup = Groups.FirstOrDefault();
        }

        private void LoadSupportPersonsForGroup()
        {
            SupportPersons.Clear();
            
            if (SelectedGroup == null) return;

            // Mock data based on selected group
            switch (SelectedGroup.Id)
            {
                case 1:
                    SupportPersons.Add(new SupportPersonModel { Id = 1, Name = "陈小二" });
                    SupportPersons.Add(new SupportPersonModel { Id = 2, Name = "李小花" });
                    break;
                case 2:
                    SupportPersons.Add(new SupportPersonModel { Id = 3, Name = "王小明" });
                    SupportPersons.Add(new SupportPersonModel { Id = 4, Name = "张小红" });
                    break;
                case 3:
                    SupportPersons.Add(new SupportPersonModel { Id = 5, Name = "刘小强" });
                    SupportPersons.Add(new SupportPersonModel { Id = 6, Name = "赵小美" });
                    break;
            }

            SelectedSupportPerson = SupportPersons.FirstOrDefault();
        }

        private async Task AssignSupportAsync()
        {
            try
            {
                if (SelectedSupportPerson == null)
                {
                    MessageBox.Show("请选择客服人员", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // TODO: Call API to assign support person to customer
                // await _apiService.AssignSupportPersonAsync(_customer.Id, SelectedSupportPerson.Id);

                // Update customer's support person
                _customer.SupportPersonName = SelectedSupportPerson.Name;

                // Publish assignment event for other modules
                EventBus.Instance.Publish(new CustomerAssignedEvent
                {
                    CustomerId = _customer.Id,
                    CustomerName = _customer.Name,
                    SupportPersonId = SelectedSupportPerson.Id,
                    SupportPersonName = SelectedSupportPerson.Name,
                    AssignedAt = DateTime.Now,
                    AssignmentType = "Support"
                });

                MessageBox.Show($"已成功将客户 '{_customer.Name}' 分配给客服 '{SelectedSupportPerson.Name}'",
                    "分配成功", MessageBoxButton.OK, MessageBoxImage.Information);

                AssignmentCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"分配客服失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }

    public class SupportPersonModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
