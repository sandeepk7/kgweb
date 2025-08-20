using System.Windows;
using System.Windows.Controls;

namespace KGWin
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void GoToMap_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to Map page using the same method as top navigation
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.NavigateToMap();
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "KGWin v1.0\n\nA modern WPF application with navigation.\n\nFeatures:\n• Top navigation bar\n• Home and Map pages\n• Modern UI design\n• Responsive layout",
                "About KGWin",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
