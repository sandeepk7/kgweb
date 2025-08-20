using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Web;

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

        private void OpenKGWeb_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create data to pass to KGWeb
                var kgWinData = new
                {
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    application = "KGWin Desktop",
                    version = "1.0",
                    features = new[] { "Offline Maps", "SignalR Communication", "Browser Extension Support" },
                    status = "Running",
                    user = Environment.UserName,
                    machine = Environment.MachineName
                };

                // Convert to JSON and URL encode
                string jsonData = System.Text.Json.JsonSerializer.Serialize(kgWinData);
                string encodedData = HttpUtility.UrlEncode(jsonData);

                // Create URL with data parameter
                string kgWebUrl = $"https://sandeepk7.github.io/kgweb/?data={encodedData}";

                // Open in default browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = kgWebUrl,
                    UseShellExecute = true
                });

                MessageBox.Show(
                    $"Opening KGWeb application with KGWin data:\n\n{jsonData}",
                    "Opening KGWeb",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening KGWeb: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
