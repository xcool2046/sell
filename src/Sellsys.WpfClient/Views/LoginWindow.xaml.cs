using Sellsys.WpfClient.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace Sellsys.WpfClient.Views
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();

            // 设置焦点到用户名输入框
            Loaded += (s, e) => UsernameTextBox.Focus();

            // 监听密码框变化，同步到ViewModel
            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // 只有在未登录状态下才退出应用程序
            if (!Services.CurrentUser.IsLoggedIn)
            {
                Application.Current.Shutdown();
            }
            else
            {
                this.Close();
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // 同步密码到ViewModel
            var viewModel = DataContext as LoginViewModel;
            if (viewModel != null)
            {
                viewModel.Password = PasswordBox.Password;
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // 当在密码框中按回车时，执行登录命令
                var viewModel = DataContext as LoginViewModel;
                if (viewModel?.LoginCommand.CanExecute(null) == true)
                {
                    // 将密码传递给ViewModel
                    viewModel.Password = PasswordBox.Password;
                    viewModel.LoginCommand.Execute(null);
                }
            }
        }

        // 当窗口关闭时的处理
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // 只有在未登录状态下才退出应用程序
            if (!Services.CurrentUser.IsLoggedIn)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
