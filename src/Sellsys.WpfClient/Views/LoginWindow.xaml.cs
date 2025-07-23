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
        private bool _isLoginSuccessful = false;

        public LoginWindow()
        {
            InitializeComponent();
            var viewModel = new LoginViewModel();
            DataContext = viewModel;

            // 监听登录成功事件
            viewModel.LoginSuccessful += OnLoginSuccessful;

            // 设置焦点到用户名输入框
            Loaded += (s, e) => UsernameTextBox.Focus();

            // 监听密码框变化，同步到ViewModel
            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }

        private void OnLoginSuccessful()
        {
            _isLoginSuccessful = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // 关闭登录窗口时退出应用程序
            Application.Current.Shutdown();
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
            // 只有在登录不成功的情况下才退出应用程序
            if (!_isLoginSuccessful)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
