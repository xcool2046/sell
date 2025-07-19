using System.Windows;

namespace Sellsys.WpfClient
{
    public partial class TestApp : Window
    {
        public TestApp()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
