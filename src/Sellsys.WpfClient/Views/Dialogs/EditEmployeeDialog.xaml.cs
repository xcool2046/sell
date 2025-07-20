using Sellsys.WpfClient.ViewModels.Dialogs;
using System.Windows;

namespace Sellsys.WpfClient.Views.Dialogs
{
    /// <summary>
    /// EditEmployeeDialog.xaml 的交互逻辑
    /// </summary>
    public partial class EditEmployeeDialog : Window
    {
        public EditEmployeeDialog()
        {
            InitializeComponent();
        }

        public EditEmployeeDialog(EditEmployeeDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
            
            // 订阅关闭事件
            viewModel.RequestClose += (sender, e) => Close();
        }
    }
}
