using Sellsys.WpfClient.ViewModels.Dialogs;
using System.Windows;

namespace Sellsys.WpfClient.Views.Dialogs
{
    /// <summary>
    /// EditDepartmentDialog.xaml 的交互逻辑
    /// </summary>
    public partial class EditDepartmentDialog : Window
    {
        public EditDepartmentDialog()
        {
            InitializeComponent();
            
            // 设置焦点到部门名称输入框
            Loaded += (s, e) => DepartmentNameTextBox.Focus();
        }

        public EditDepartmentDialog(EditDepartmentDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
            
            // 订阅关闭事件
            viewModel.RequestClose += (sender, e) => Close();
        }
    }
}
