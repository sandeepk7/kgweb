using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Web;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Text;

namespace KGWin
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        // Windows API declarations for finding browser windows
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        private const int SW_RESTORE = 9;

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
                    message = "KGWin Desktop Application Connected Successfully",
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                // Convert to JSON and URL encode
                string jsonData = System.Text.Json.JsonSerializer.Serialize(kgWinData);
                string encodedData = HttpUtility.UrlEncode(jsonData);

                // Create URL with data parameter
                string kgWebUrl = $"https://sandeepk7.github.io/kgweb/?data={encodedData}";

                // Try to find existing KGWeb tab first
                if (TryActivateExistingKGWebTab())
                {
                    // Existing tab found and activated
                    // Just bring the browser window to front - don't try to navigate
                    System.Diagnostics.Debug.WriteLine("Existing KGWeb tab activated");
                }
                else
                {
                    // No existing tab found, open new browser window/tab
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = kgWebUrl,
                        UseShellExecute = true
                    });
                }
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

        private bool TryActivateExistingKGWebTab()
        {
            try
            {
                // Common browser window class names
                string[] browserClasses = { "Chrome_WidgetWin_1", "MozillaWindowClass", "IEFrame" };
                
                foreach (string className in browserClasses)
                {
                    IntPtr browserWindow = FindWindow(className, null);
                    if (browserWindow != IntPtr.Zero)
                    {
                        // Check if this browser window has KGWeb tab
                        if (HasKGWebTab(browserWindow))
                        {
                            // Activate the browser window
                            if (!IsWindowVisible(browserWindow))
                            {
                                ShowWindow(browserWindow, SW_RESTORE);
                            }
                            SetForegroundWindow(browserWindow);
                            return true;
                        }
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking for existing KGWeb tab: {ex.Message}");
                return false;
            }
        }

        private bool HasKGWebTab(IntPtr browserWindow)
        {
            try
            {
                StringBuilder windowTitle = new StringBuilder(256);
                GetWindowText(browserWindow, windowTitle, windowTitle.Capacity);
                string title = windowTitle.ToString().ToLower();
                
                // Check if the window title contains KGWeb indicators
                return title.Contains("kgweb") || 
                       title.Contains("sandeepk7.github.io") || 
                       title.Contains("kgweb");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking window title: {ex.Message}");
                return false;
            }
        }


    }
}
