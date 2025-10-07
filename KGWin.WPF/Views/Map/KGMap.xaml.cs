using Esri.ArcGISRuntime.UI.Controls;
using KGWin.WPF.ViewModels;
using System.Windows.Controls;

namespace KGWin.WPF.Views.Map
{
    /// <summary>
    /// Interaction logic for KGMapView.xaml
    /// </summary>
    public partial class KGMap : UserControl
    {
        public KGMap()
        {
            InitializeComponent();            
        }

        private async void MapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            if (DataContext is KGMapViewModel viewModel)
            {
                await viewModel.HandleMapClick(KGMapView, e);
            }
        }
    }
}
