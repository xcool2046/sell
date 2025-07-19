using System.Windows;
using Sellsys.WpfClient.ViewModels;

namespace Sellsys.WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            // 强制显示窗口
            this.Show();
            this.Activate();
            this.Focus();
            this.BringIntoView();

            // 确保窗口在屏幕中央
            this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
            this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;
        }
    }
}