using Sellsys.WpfClient.ViewModels.Dialogs;
using System.Windows;

namespace Sellsys.WpfClient.Views.Dialogs
{
    /// <summary>
    /// EditPermissionDialog.xaml 的交互逻辑
    /// </summary>
    public partial class EditPermissionDialog : Window
    {
        public EditPermissionDialog()
        {
            InitializeComponent();
        }

        public EditPermissionDialog(EditPermissionDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
            
            // 订阅关闭事件
            viewModel.RequestClose += (sender, e) => Close();
        }
    }
}
