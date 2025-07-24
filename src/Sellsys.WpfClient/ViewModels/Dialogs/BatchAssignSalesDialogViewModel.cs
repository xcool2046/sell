using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Sellsys.WpfClient.ViewModels.Dialogs
{
    public class BatchAssignSalesDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly List<Customer> _customers;
        private bool _isLoading;
        private ObservableCollection<DepartmentGroup> _groups;
        private ObservableCollection<Employee> _salesPersons;
        private DepartmentGroup? _selectedGroup;
        private Employee? _selectedSalesPerson;

        public ObservableCollection<Customer> SelectedCustomers { get; }
        
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ObservableCollection<DepartmentGroup> Groups
        {
            get => _groups;
            set => SetProperty(ref _groups, value);
        }

        public ObservableCollection<Employee> SalesPersons
        {
            get => _salesPersons;
            set => SetProperty(ref _salesPersons, value);
        }

        public DepartmentGroup? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value))
                {
                    _ = LoadSalesPersonsAsync();
                }
            }
        }

        public Employee? SelectedSalesPerson
        {
            get => _selectedSalesPerson;
            set => SetProperty(ref _selectedSalesPerson, value);
        }

        public ICommand AssignCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler? AssignmentCompleted;
        public event EventHandler? Cancelled;

        public BatchAssignSalesDialogViewModel(ApiService apiService, List<Customer> customers)
        {
            _apiService = apiService;
            _customers = customers;
            
            SelectedCustomers = new ObservableCollection<Customer>(customers);
            _groups = new ObservableCollection<DepartmentGroup>();
            _salesPersons = new ObservableCollection<Employee>();

            AssignCommand = new AsyncRelayCommand(async p => await AssignSalesPersonAsync());
            CancelCommand = new RelayCommand(p => Cancel());

            _ = InitializeDataAsync();
        }

        private async Task InitializeDataAsync()
        {
            try
            {
                IsLoading = true;

                // Get all departments to find sales department ID (keeping original logic for batch operations)
                var departments = await _apiService.GetDepartmentsAsync();
                var salesDepartment = departments.FirstOrDefault(d => d.Name == "销售部");

                if (salesDepartment != null)
                {
                    // Load groups for sales department
                    var groups = await _apiService.GetDepartmentGroupsByDepartmentIdAsync(salesDepartment.Id);
                    Groups.Clear();
                    foreach (var group in groups)
                    {
                        Groups.Add(group);
                    }

                    // Set default selection
                    SelectedGroup = Groups.FirstOrDefault();
                }
                else
                {
                    MessageBox.Show("未找到销售部门，请先在系统设置中创建销售部门", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadSalesPersonsAsync()
        {
            if (SelectedGroup == null) return;

            try
            {
                IsLoading = true;

                var employees = await _apiService.GetEmployeesByGroupIdAsync(SelectedGroup.Id);
                SalesPersons.Clear();
                foreach (var employee in employees)
                {
                    SalesPersons.Add(employee);
                }

                // Set default selection
                SelectedSalesPerson = SalesPersons.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载销售人员失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AssignSalesPersonAsync()
        {
            if (SelectedSalesPerson == null)
            {
                MessageBox.Show("请选择销售人员", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_customers.Any())
            {
                MessageBox.Show("没有选择客户", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;

                var successCount = 0;
                var failedCustomers = new List<string>();

                foreach (var customer in _customers)
                {
                    try
                    {
                        // Call API to assign sales person to customer
                        await _apiService.AssignSalesPersonAsync(customer.Id, SelectedSalesPerson.Id);

                        // Update customer's sales person
                        customer.SalesPersonId = SelectedSalesPerson.Id;
                        customer.SalesPersonName = SelectedSalesPerson.Name;

                        // Publish assignment event for other modules
                        EventBus.Instance.Publish(new CustomerAssignedEvent
                        {
                            CustomerId = customer.Id,
                            CustomerName = customer.Name,
                            SalesPersonId = SelectedSalesPerson.Id,
                            SalesPersonName = SelectedSalesPerson.Name,
                            AssignedAt = DateTime.Now,
                            AssignmentType = "Sales"
                        });

                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failedCustomers.Add($"{customer.Name}: {ex.Message}");
                    }
                }

                // Show result message only if there are failures
                if (failedCustomers.Any())
                {
                    var message = $"批量分配完成！\n成功: {successCount} 个客户\n失败: {failedCustomers.Count} 个客户\n\n失败详情:\n{string.Join("\n", failedCustomers)}";
                    MessageBox.Show(message, "分配结果", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                // Remove success message - no popup for successful operations

                AssignmentCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"批量分配失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
}
