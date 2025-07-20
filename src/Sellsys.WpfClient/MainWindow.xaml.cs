using System.Windows;
using Sellsys.WpfClient.ViewModels;
using System.IO;

namespace Sellsys.WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                // 写入日志文件
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "wpf_debug.log");
                File.AppendAllText(logPath, $"{DateTime.Now}: MainWindow: Starting initialization...\n");

                System.Diagnostics.Debug.WriteLine("MainWindow: Starting initialization...");

                InitializeComponent();
                System.Diagnostics.Debug.WriteLine("MainWindow: InitializeComponent completed");
                File.AppendAllText(logPath, $"{DateTime.Now}: MainWindow: InitializeComponent completed\n");

                DataContext = new MainViewModel();
                System.Diagnostics.Debug.WriteLine("MainWindow: MainViewModel created and set as DataContext");
                File.AppendAllText(logPath, $"{DateTime.Now}: MainWindow: MainViewModel created and set as DataContext\n");

                // 强制显示窗口
                this.Show();
                System.Diagnostics.Debug.WriteLine("MainWindow: Show() called");
                File.AppendAllText(logPath, $"{DateTime.Now}: MainWindow: Show() called\n");

                this.Activate();
                System.Diagnostics.Debug.WriteLine("MainWindow: Activate() called");

                this.Focus();
                System.Diagnostics.Debug.WriteLine("MainWindow: Focus() called");

                this.BringIntoView();
                System.Diagnostics.Debug.WriteLine("MainWindow: BringIntoView() called");

                // 确保窗口在屏幕中央
                this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
                this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;
                System.Diagnostics.Debug.WriteLine($"MainWindow: Window positioned at ({this.Left}, {this.Top})");

                System.Diagnostics.Debug.WriteLine("MainWindow: Initialization completed successfully");
                File.AppendAllText(logPath, $"{DateTime.Now}: MainWindow: Initialization completed successfully\n");
            }
            catch (Exception ex)
            {
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "wpf_debug.log");
                File.AppendAllText(logPath, $"{DateTime.Now}: MainWindow: Exception during initialization: {ex.Message}\n");
                File.AppendAllText(logPath, $"{DateTime.Now}: MainWindow: Stack trace: {ex.StackTrace}\n");

                System.Diagnostics.Debug.WriteLine($"MainWindow: Exception during initialization: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"MainWindow: Stack trace: {ex.StackTrace}");
                MessageBox.Show($"主窗口初始化失败: {ex.Message}\n\n{ex.StackTrace}", "初始化错误", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
    }
}