using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using KGWin.WPF.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

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
        }

        //GraphicsOverlay _drawOverlay;
        //Graphic _inProgressGraphic;
        //List<MapPoint> _tempPoints = new List<MapPoint>();
        //bool _isDrawing = false;


        private async void KGMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            if (DataContext is KGMapViewModel viewModel)
            {
                await viewModel.HandleMapClick(KGMapView, e);
            }
        }

        //void InitializeDrawingOverlay()
        //{
        //    _drawOverlay = new GraphicsOverlay();
        //    KGMapView.GraphicsOverlays.Add(_drawOverlay);

        //    // Create a graphic with initial empty geometry
        //    _inProgressGraphic = new Graphic
        //    {
        //        Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 2)
        //    };
        //    _drawOverlay.Graphics.Add(_inProgressGraphic);
        //}

        //void OnMouseDown(object sender, MouseEventArgs e)
        //{
        //    _isDrawing = true;
        //    _tempPoints.Clear();

        //    var mp = KGMapView.ScreenToLocation(e.GetPosition(KGMapView));
        //    _tempPoints.Add(mp);
        //}

        //void OnMouseMove(object sender, MouseEventArgs e)
        //{
        //    if (!_isDrawing)
        //        return;

        //    var mp = KGMapView.ScreenToLocation(e.GetPosition(KGMapView));
        //    _tempPoints.Add(mp);

        //    // Update the graphic geometry with the current polyline
        //    var polyline = new Polyline(_tempPoints, KGMapView.SpatialReference);
        //    _inProgressGraphic.Geometry = polyline;
        //}

        //async void OnMouseUp(object sender, MouseEventArgs e)
        //{
        //    _isDrawing = false;

        //    if (_tempPoints.Count < 3)
        //    {
        //        // too few points — cancel or ignore
        //        return;
        //    }

        //    // Close polygon by adding first point again
        //    _tempPoints.Add(_tempPoints[0]);

        //    var polygon = new Polygon(_tempPoints, KGMapView.SpatialReference);

        //    // Update the graphic symbol to fill or shape
        //    _inProgressGraphic.Symbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid,
        //                                       System.Drawing.Color.FromArgb(100, System.Drawing.Color.Red),
        //                                       new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 2));
        //    _inProgressGraphic.Geometry = polygon;

        //    // Optionally perform selection or query using that polygon
        //    var layer = KGMapView.Map.OperationalLayers.OfType<FeatureLayer>().FirstOrDefault();
        //    if (layer != null)
        //    {
        //        //var result = await layer.SelectFeaturesAsync(
        //        //    polygon,                    
        //        //    Esri.ArcGISRuntime.Mapping.SelectionMode.New,
        //        //    QueryType.Geometry,
        //        //    SpatialRelationship.Contains);
        //        // handle result
        //    }
        //}

    }
}
