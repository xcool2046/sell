using Sellsys.WpfClient.ViewModels.Dialogs;
using System.Windows;

namespace Sellsys.WpfClient.Views.Dialogs
{
    /// <summary>
    /// AddEmployeeDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddEmployeeDialog : Window
    {
        public AddEmployeeDialog()
        {
            InitializeComponent();
        }

        public AddEmployeeDialog(AddEmployeeDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
            
            // 订阅关闭事件
            viewModel.RequestClose += (sender, e) => Close();
        }
    }
}
