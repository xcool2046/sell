using Sellsys.WpfClient.ViewModels.Dialogs;
using System.Windows;

namespace Sellsys.WpfClient.Views.Dialogs
{
    /// <summary>
    /// AddPermissionDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddPermissionDialog : Window
    {
        public AddPermissionDialog()
        {
            InitializeComponent();
        }

        public AddPermissionDialog(AddPermissionDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
            
            // 订阅关闭事件
            viewModel.RequestClose += (sender, e) => Close();
        }
    }
}
