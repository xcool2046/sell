using Sellsys.WpfClient.ViewModels.Dialogs;
using System.Windows;

namespace Sellsys.WpfClient.Views.Dialogs
{
    /// <summary>
    /// AddEditFeedbackDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddEditFeedbackDialog : Window
    {
        public AddEditFeedbackDialog()
        {
            InitializeComponent();
        }

        public AddEditFeedbackDialog(AddEditFeedbackDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
            
            // 订阅关闭事件
            viewModel.RequestClose += (sender, e) => Close();
        }
    }
}
