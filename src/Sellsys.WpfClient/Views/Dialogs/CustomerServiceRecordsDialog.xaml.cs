using Sellsys.WpfClient.ViewModels.Dialogs;
using System.Windows;

namespace Sellsys.WpfClient.Views.Dialogs
{
    /// <summary>
    /// CustomerServiceRecordsDialog.xaml 的交互逻辑
    /// </summary>
    public partial class CustomerServiceRecordsDialog : Window
    {
        public CustomerServiceRecordsDialog()
        {
            InitializeComponent();
        }

        public CustomerServiceRecordsDialog(CustomerServiceRecordsDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
            
            // 订阅关闭事件
            viewModel.RequestClose += (sender, e) => Close();
        }
    }
}
