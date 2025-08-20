using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KGWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HomePage? homePage;
        private MapPage? mapPage;
        private SignalRCommunicationPage? signalRMonitorPage;

        public MainWindow()
        {
            InitializeComponent();
            InitializePages();
            NavigateToHome();
        }

        private void InitializePages()
        {
            homePage = new HomePage();
        }

        private void NavigateToHome()
        {
            MainFrame.Navigate(homePage);
            UpdateNavigationButtons(HomeButton);
        }

        public void NavigateToMap()
        {
            mapPage = new MapPage();
            MainFrame.Navigate(mapPage);
            UpdateNavigationButtons(MapButton);
        }

        private void NavigateToSignalRMonitor()
        {
            signalRMonitorPage = new SignalRCommunicationPage();
            MainFrame.Navigate(signalRMonitorPage);
            UpdateNavigationButtons(SignalRMonitorButton);
        }



        private void UpdateNavigationButtons(Button activeButton, params Button[] inactiveButtons)
        {
            // Reset all buttons to default style
            HomeButton.Style = (Style)FindResource("NavButtonStyle");
            MapButton.Style = (Style)FindResource("NavButtonStyle");
            SignalRMonitorButton.Style = (Style)FindResource("NavButtonStyle");
            
            // Set active button style
            activeButton.Style = (Style)FindResource("ActiveNavButtonStyle");
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToHome();
        }

        private void MapButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToMap();
        }

        private void SignalRMonitorButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToSignalRMonitor();
        }


    }
}
