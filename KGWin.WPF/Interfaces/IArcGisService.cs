using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using KGWin.WPF.ViewModels;
using System.Windows;

namespace KGWin.WPF.Interfaces
{
    public interface IArcGisService
    {
        void Initialize();
        LayerItemViewModel ExtractLayerMetadata(Layer layer);
        Task<Dictionary<string, string>> ExtractMapObjectDataOnClickPoint(MapView mapView, Point position, KGPopupViewModel kgPopupVM);
    }
}
