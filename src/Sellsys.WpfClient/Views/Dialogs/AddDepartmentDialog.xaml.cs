using Sellsys.WpfClient.ViewModels.Dialogs;
using System.Windows;

namespace Sellsys.WpfClient.Views.Dialogs
{
    /// <summary>
    /// AddDepartmentDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddDepartmentDialog : Window
    {
        public AddDepartmentDialog()
        {
            InitializeComponent();
            
            // 设置焦点到部门名称输入框
            Loaded += (s, e) => DepartmentNameTextBox.Focus();
        }

        public AddDepartmentDialog(AddDepartmentDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
            
            // 订阅关闭事件
            viewModel.RequestClose += (sender, e) => Close();
        }
    }
}
