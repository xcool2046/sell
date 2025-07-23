using Sellsys.WpfClient.Commands;
using Sellsys.WpfClient.Models;
using Sellsys.WpfClient.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Sellsys.WpfClient.ViewModels.Dialogs
{
    public class AddEditProductDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly Product? _existingProduct;
        private readonly bool _isEditMode;

        private string _productName = string.Empty;
        private string _specification = string.Empty;
        private string _unit = string.Empty;
        private string _selectedUnit = string.Empty;
        private string _listPrice = string.Empty;
        private string _minPrice = string.Empty;
        private string _salesCommission = string.Empty;
        private string _supervisorCommission = string.Empty;
        private string _managerCommission = string.Empty;
        private bool _isSaving;

        public string DialogTitle => _isEditMode ? "编辑产品" : "添加产品";

        public List<string> AvailableUnits { get; } = new List<string>
        {
            "套", "个", "台", "件", "箱", "包", "米", "千克", "升", "平方米", "立方米"
        };

        [Required(ErrorMessage = "产品名称不能为空")]
        [StringLength(255, ErrorMessage = "产品名称长度不能超过255个字符")]
        public string ProductName
        {
            get => _productName;
            set => SetProperty(ref _productName, value);
        }

        [StringLength(100, ErrorMessage = "型号规格长度不能超过100个字符")]
        public string Specification
        {
            get => _specification;
            set => SetProperty(ref _specification, value);
        }

        [StringLength(20, ErrorMessage = "计量单位长度不能超过20个字符")]
        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        public string SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                SetProperty(ref _selectedUnit, value);
                if (!string.IsNullOrEmpty(value))
                {
                    Unit = value;
                }
            }
        }

        [Required(ErrorMessage = "产品定价不能为空")]
        public string ListPrice
        {
            get => _listPrice;
            set => SetProperty(ref _listPrice, value);
        }

        [Required(ErrorMessage = "最低控价不能为空")]
        public string MinPrice
        {
            get => _minPrice;
            set => SetProperty(ref _minPrice, value);
        }

        public string SalesCommission
        {
            get => _salesCommission;
            set => SetProperty(ref _salesCommission, value);
        }

        public string SupervisorCommission
        {
            get => _supervisorCommission;
            set => SetProperty(ref _supervisorCommission, value);
        }

        public string ManagerCommission
        {
            get => _managerCommission;
            set => SetProperty(ref _managerCommission, value);
        }

        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler? ProductSaved;
        public event EventHandler? Cancelled;
        public event EventHandler? RequestClose;

        public AddEditProductDialogViewModel(ApiService apiService, Product? existingProduct = null)
        {
            _apiService = apiService;
            _existingProduct = existingProduct;
            _isEditMode = existingProduct != null;

            SaveCommand = new AsyncRelayCommand(async p => await SaveProductAsync(), p => CanSave());
            CancelCommand = new RelayCommand(p => Cancel());

            if (_isEditMode && _existingProduct != null)
            {
                LoadExistingProductData();
            }
        }

        private void LoadExistingProductData()
        {
            if (_existingProduct == null) return;

            ProductName = _existingProduct.Name;
            Specification = _existingProduct.Specification ?? string.Empty;
            Unit = _existingProduct.Unit ?? string.Empty;
            SelectedUnit = _existingProduct.Unit ?? string.Empty;
            ListPrice = _existingProduct.ListPrice.ToString("F2");
            MinPrice = _existingProduct.MinPrice.ToString("F2");
            SalesCommission = _existingProduct.SalesCommission?.ToString("F2") ?? string.Empty;
            SupervisorCommission = _existingProduct.SupervisorCommission?.ToString("F2") ?? string.Empty;
            ManagerCommission = _existingProduct.ManagerCommission?.ToString("F2") ?? string.Empty;
        }

        private bool CanSave()
        {
            return !IsSaving && 
                   !string.IsNullOrWhiteSpace(ProductName) && 
                   !string.IsNullOrWhiteSpace(ListPrice) && 
                   !string.IsNullOrWhiteSpace(MinPrice);
        }

        private async Task SaveProductAsync()
        {
            try
            {
                IsSaving = true;

                // Validate input
                if (!ValidateInput())
                {
                    return;
                }

                var productDto = CreateProductDto();

                if (_isEditMode && _existingProduct != null)
                {
                    await _apiService.UpdateProductAsync(_existingProduct.Id, productDto);

                    // 发布产品更新事件
                    EventBus.Instance.Publish(new ProductUpdatedEvent
                    {
                        ProductId = _existingProduct.Id,
                        ProductName = ProductName,
                        UpdateType = "Updated",
                        UpdatedAt = DateTime.Now
                    });
                }
                else
                {
                    var newProduct = await _apiService.CreateProductAsync(productDto);

                    // 发布产品创建事件
                    EventBus.Instance.Publish(new ProductUpdatedEvent
                    {
                        ProductId = newProduct?.Id ?? 0,
                        ProductName = ProductName,
                        UpdateType = "Created",
                        UpdatedAt = DateTime.Now
                    });
                }

                ProductSaved?.Invoke(this, EventArgs.Empty);
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存产品失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(ProductName))
            {
                MessageBox.Show("产品名称不能为空", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(ListPrice, out var listPrice) || listPrice <= 0)
            {
                MessageBox.Show("产品定价必须是大于0的数字", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(MinPrice, out var minPrice) || minPrice <= 0)
            {
                MessageBox.Show("最低控价必须是大于0的数字", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (minPrice > listPrice)
            {
                MessageBox.Show("最低控价不能高于产品定价", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validate optional commission fields
            if (!string.IsNullOrWhiteSpace(SalesCommission) && (!decimal.TryParse(SalesCommission, out var salesComm) || salesComm < 0))
            {
                MessageBox.Show("销售提成必须是非负数字", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(SupervisorCommission) && (!decimal.TryParse(SupervisorCommission, out var supComm) || supComm < 0))
            {
                MessageBox.Show("主管提成必须是非负数字", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(ManagerCommission) && (!decimal.TryParse(ManagerCommission, out var mgComm) || mgComm < 0))
            {
                MessageBox.Show("经理提成必须是非负数字", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private ProductUpsertDto CreateProductDto()
        {
            return new ProductUpsertDto
            {
                Name = ProductName.Trim(),
                Specification = string.IsNullOrWhiteSpace(Specification) ? null : Specification.Trim(),
                Unit = string.IsNullOrWhiteSpace(Unit) ? null : Unit.Trim(),
                ListPrice = decimal.Parse(ListPrice),
                MinPrice = decimal.Parse(MinPrice),
                SalesCommission = string.IsNullOrWhiteSpace(SalesCommission) ? null : decimal.Parse(SalesCommission),
                SupervisorCommission = string.IsNullOrWhiteSpace(SupervisorCommission) ? null : decimal.Parse(SupervisorCommission),
                ManagerCommission = string.IsNullOrWhiteSpace(ManagerCommission) ? null : decimal.Parse(ManagerCommission)
            };
        }

        private void Cancel()
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
