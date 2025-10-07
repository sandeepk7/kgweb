using KGWin.WPF.Interfaces;
using KGWin.WPF.Services;
using KGWin.WPF.ViewModels;
using KGWin.WPF.ViewModels.Base;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KGWin.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //_arcGisLoginService = arcGisLoginService;
            //_kGButtonViewModel = kGButtonViewModel;
            //_kGButtonViewModel.ButtonContent = "Login";
            //_kGButtonViewModel.ButtonCommand = new RelayCommand(Login_btn);

        }
        //KGButtonViewModel _kGButtonViewModel;
        //IArcGisLoginService _arcGisLoginService;
       
    }
}
