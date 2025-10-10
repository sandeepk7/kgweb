using KGWin.WPF.ViewModels;
using KGWin.WPF.ViewModels.Base;
using System.Windows.Controls;

namespace KGWin.WPF.Views.Map
{
    /// <summary>
    /// Interaction logic for KGKGMapView.xaml
    /// </summary>
    public partial class KGMap : UserControl
    {
        public KGMap()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            var vm = (KGMapViewModel)DataContext;
            vm.AssiginView(KGMapView);

            var eventAggregator = vm.EventAggregator;

            KGMapView.GeoViewTapped += (sender, eventArg) => eventAggregator.Publish(new KGEvent(nameof(KGMapView.GeoViewTapped), sender, eventArg));
        }
    }
}
