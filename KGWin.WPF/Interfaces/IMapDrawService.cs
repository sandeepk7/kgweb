using Esri.ArcGISRuntime.UI.Controls;
using System.Windows;

namespace KGWin.WPF.Interfaces
{
    public interface IMapDrawService
    {
        bool IsDrawingModeOn { get; }
        void Initialize(MapView mapView);
        void OnLassoSelectClick(object sender, RoutedEventArgs e);
    }
}
