using Sellsys.WpfClient.ViewModels.Dialogs;
using System.Windows;

namespace Sellsys.WpfClient.Views.Dialogs
{
    /// <summary>
    /// AddEditProductDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddEditProductDialog : Window
    {
        public AddEditProductDialog()
        {
            InitializeComponent();
        }

        public AddEditProductDialog(AddEditProductDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
            
            // 订阅关闭事件
            viewModel.RequestClose += (sender, e) => Close();
        }
    }
}
