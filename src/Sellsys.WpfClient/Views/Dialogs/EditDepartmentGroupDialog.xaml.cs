using Sellsys.WpfClient.ViewModels.Dialogs;
using System.Windows;

namespace Sellsys.WpfClient.Views.Dialogs
{
    /// <summary>
    /// EditDepartmentGroupDialog.xaml 的交互逻辑
    /// </summary>
    public partial class EditDepartmentGroupDialog : Window
    {
        public EditDepartmentGroupDialog()
        {
            InitializeComponent();
            
            // 设置焦点到部门下拉框
            Loaded += (s, e) => DepartmentComboBox.Focus();
        }

        public EditDepartmentGroupDialog(EditDepartmentGroupDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
            
            // 订阅关闭事件
            viewModel.RequestClose += (sender, e) => Close();
        }
    }
}
