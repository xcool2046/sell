using Sellsys.WpfClient.Services;
using Sellsys.WpfClient.Commands;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Text.Json;

namespace Sellsys.WpfClient.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthenticationService _authService;
        private readonly string _settingsFilePath;

        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _rememberUsername = false;
        private bool _isLoading = false;

        public event Action? LoginSuccessful;

        public LoginViewModel()
        {
            var apiService = new ApiService();
            _authService = new AuthenticationService(apiService);
            
            // 设置配置文件路径
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "SellsysClient");
            Directory.CreateDirectory(appFolder);
            _settingsFilePath = Path.Combine(appFolder, "login_settings.json");

            LoginCommand = new AsyncRelayCommand(async p => await LoginAsync(), p => !IsLoading);
            
            // 加载保存的设置
            LoadSettings();
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public bool RememberUsername
        {
            get => _rememberUsername;
            set
            {
                _rememberUsername = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand LoginCommand { get; }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                MessageBox.Show("请输入用户名", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("请输入密码", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;

            try
            {
                System.Diagnostics.Debug.WriteLine($"开始登录: 用户名={Username}");

                var result = await _authService.LoginAsync(Username, Password);

                System.Diagnostics.Debug.WriteLine($"登录结果: Success={result.Success}, Message={result.Message}");

                if (result.Success)
                {
                    System.Diagnostics.Debug.WriteLine("登录成功，准备显示主窗口");

                    // 保存设置
                    SaveSettings();

                    // 触发登录成功事件
                    LoginSuccessful?.Invoke();

                    // 登录成功，关闭登录窗口并打开主窗口
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine("=== 开始创建主窗口 ===");

                            var mainWindow = new MainWindow();
                            System.Diagnostics.Debug.WriteLine("主窗口对象创建成功");

                            System.Diagnostics.Debug.WriteLine("设置主窗口属性...");
                            mainWindow.WindowState = WindowState.Maximized;
                            mainWindow.ShowInTaskbar = true;
                            mainWindow.Topmost = false;

                            System.Diagnostics.Debug.WriteLine("设置主窗口为应用程序主窗口...");
                            Application.Current.MainWindow = mainWindow;

                            System.Diagnostics.Debug.WriteLine("显示主窗口...");
                            mainWindow.Show();

                            System.Diagnostics.Debug.WriteLine("激活主窗口...");
                            mainWindow.Activate();
                            mainWindow.Focus();

                            // 延迟关闭登录窗口
                            System.Diagnostics.Debug.WriteLine("准备关闭登录窗口...");
                            var loginWindow = Application.Current.Windows.OfType<Views.LoginWindow>().FirstOrDefault();
                            if (loginWindow != null)
                            {
                                loginWindow.Close();
                                System.Diagnostics.Debug.WriteLine("登录窗口已关闭");
                            }

                            System.Diagnostics.Debug.WriteLine("=== 登录流程完成 ===");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"创建主窗口时发生错误: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                            MessageBox.Show($"创建主窗口时发生错误：{ex.Message}\n\n堆栈跟踪：{ex.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"登录失败: {result.Message}");
                    MessageBox.Show(result.Message, "登录失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"登录异常: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                MessageBox.Show($"登录过程中发生错误：{ex.Message}\n\n详细信息：{ex.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<LoginSettings>(json);

                    if (settings != null)
                    {
                        RememberUsername = settings.RememberUsername;
                        if (RememberUsername)
                        {
                            Username = settings.Username ?? string.Empty;
                            Password = settings.Password ?? string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载登录设置失败: {ex.Message}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new LoginSettings
                {
                    RememberUsername = RememberUsername,
                    Username = RememberUsername ? Username : string.Empty,
                    Password = RememberUsername ? Password : string.Empty
                };

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存登录设置失败: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // 登录设置类
    public class LoginSettings
    {
        public bool RememberUsername { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
