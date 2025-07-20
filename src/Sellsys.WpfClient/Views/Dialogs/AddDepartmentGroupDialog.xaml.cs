using Sellsys.WpfClient.ViewModels.Dialogs;
using System.Windows;

namespace Sellsys.WpfClient.Views.Dialogs
{
    /// <summary>
    /// AddDepartmentGroupDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddDepartmentGroupDialog : Window
    {
        public AddDepartmentGroupDialog()
        {
            InitializeComponent();
            
            // 设置焦点到部门下拉框
            Loaded += (s, e) => DepartmentComboBox.Focus();
        }

        public AddDepartmentGroupDialog(AddDepartmentGroupDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
            
            // 订阅关闭事件
            viewModel.RequestClose += (sender, e) => Close();
        }
    }
}
