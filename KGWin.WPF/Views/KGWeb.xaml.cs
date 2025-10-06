using KGWin.WPF.ViewModels;
using System.Windows.Controls;

namespace KGWin.WPF.Views
{
    /// <summary>
    /// Interaction logic for Web.xaml
    /// </summary>
    public partial class KGWeb : UserControl
    {
        public KGWeb()
        {
            InitializeComponent();
            var _ = _viewModel.InitializeAsync(KGWebView);
        }

        private KGWebViewModel _viewModel => (KGWebViewModel)KGWebView.DataContext;
    }
}
