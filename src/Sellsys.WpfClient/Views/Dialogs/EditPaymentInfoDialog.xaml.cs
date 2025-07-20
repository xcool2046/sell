using System.Windows;
using Sellsys.WpfClient.ViewModels.Dialogs;

namespace Sellsys.WpfClient.Views.Dialogs
{
    /// <summary>
    /// EditPaymentInfoDialog.xaml 的交互逻辑
    /// </summary>
    public partial class EditPaymentInfoDialog : Window
    {
        public EditPaymentInfoDialog()
        {
            InitializeComponent();
        }

        public EditPaymentInfoDialog(EditPaymentInfoDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
            
            // 订阅ViewModel的关闭事件
            viewModel.CloseRequested += (sender, result) =>
            {
                DialogResult = result;
                Close();
            };
        }
    }
}
