using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.Editing;
using KGWin.WPF.Interfaces;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace KGWin.WPF.Services
{
    public class MapDrawService : IMapDrawService
    {
        public MapDrawService()
        {
            _lineStyle = new(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Blue, 2);
            _fillSymbol = new(SimpleFillSymbolStyle.Null, System.Drawing.Color.Transparent, this._lineStyle);
        }

        private MapView _mapView = default!;
        private GeometryEditor _geometryEditor = default!;
        public bool IsDrawingModeOn { get; private set; }

        private bool _isMouseDown = false;
        private Point _startPoint;
        private const double DragThreshold = 3; // pixels to detect drag for freehand


        // Reuse your line style
        SimpleLineSymbol _lineStyle;

        // Create a fill symbol with transparent interior
        SimpleFillSymbol _fillSymbol;


        public void Initialize(MapView mapView)
        {
            _mapView = mapView;

            if (_mapView.GeometryEditor == null)
            {
                _mapView.GeometryEditor = new GeometryEditor();
            }
            _geometryEditor = _mapView.GeometryEditor;

            // Listen to changes (IsStarted, Geometry, etc.)
            _geometryEditor.PropertyChanged += GeometryEditor_PropertyChanged;
        }

        #region Lasso button click

        public void OnLassoSelectClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"Drawing {(IsDrawingModeOn ? "Ended":"Started")}");
            if (IsDrawingModeOn)
            {
                CleanUp();
                return;
            }

            IsDrawingModeOn = true;

            // Attach mouse events for dynamic tool selection
            _mapView.MouseLeftButtonDown += MapView_MouseLeftButtonDown;
            _mapView.PreviewMouseMove  += MapView_PreviewMouseMove ;
            _mapView.MouseLeftButtonUp += MapView_MouseLeftButtonUp;
            _mapView.MouseRightButtonUp += MapView_MouseRightButtonUp;
            _mapView.MouseDoubleClick += MapView_MouseDoubleClick;
        }

        #endregion

        #region Cleanup & Detach mouse events
        private void CleanUp()
        {
            Debug.WriteLine($"Cleanup Called");
            IsDrawingModeOn = false;
            DetachMouseEvents();
        }

        private void DetachMouseEvents(bool skipVertexStopEvents = false)
        {
            Debug.WriteLine($"DetachMouseEvents called with skipVertextEvent = {skipVertexStopEvents}");
            _isMouseDown = false;

            _mapView.MouseLeftButtonDown -= MapView_MouseLeftButtonDown;
            _mapView.PreviewMouseMove  -= MapView_PreviewMouseMove ;
            _mapView.MouseLeftButtonUp -= MapView_MouseLeftButtonUp;

            if (!skipVertexStopEvents)
            {
                _mapView.MouseRightButtonUp -= MapView_MouseRightButtonUp;
                _mapView.MouseDoubleClick -= MapView_MouseDoubleClick;
            }
        }



        #endregion

        #region Mouse events for dynamic tool selection

        private void MapView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine($"Mouse left down called with IsStarted = {_geometryEditor.IsStarted}");

            if (_geometryEditor.IsStarted) return;

            _isMouseDown = true;
            _startPoint = e.GetPosition(_mapView);
        }

        private void MapView_PreviewMouseMove (object sender, MouseEventArgs e)
        {
            if (!_isMouseDown || _geometryEditor.IsStarted) return;

            Debug.WriteLine($"Mouse move called with isMouseDown = {_isMouseDown}, IsStarted = {_geometryEditor.IsStarted}");

            var current = e.GetPosition(_mapView);
            double dx = current.X - _startPoint.X;
            double dy = current.Y - _startPoint.Y;

            Debug.WriteLine($"Mouse move called with isMouseDown = {_isMouseDown}, IsStarted = {_geometryEditor.IsStarted}, Distance = {Math.Sqrt(dx * dx + dy * dy)}");

            if (Math.Sqrt(dx * dx + dy * dy) > DragThreshold)
            {
                _mapView.MouseLeftButtonDown -= MapView_MouseLeftButtonDown;
                _mapView.PreviewMouseMove  -= MapView_PreviewMouseMove ;

                // User dragged → freehand lasso
                StartFreehandLasso();
            }
        }

        private void MapView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine($"Mouse left up called with isMouseDown = {_isMouseDown}, IsStarted = {_geometryEditor.IsStarted}");

            if (!_isMouseDown || _geometryEditor.IsStarted)
            {
                if (_geometryEditor.Tool is FreehandTool)
                {
                    var _ = FinishPolygonAsync();
                }
                return;
            }

            DetachMouseEvents(true);

            // Mouse up without drag → vertex polygon
            StartVertexPolygon();
            _isMouseDown = false;
        }

        private void MapView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine($"Mouse right up called with geometry too  = {_geometryEditor.Tool.GetType().Name}, IsStarted = {_geometryEditor.IsStarted}");

            if (_geometryEditor.IsStarted && _geometryEditor.Tool is VertexTool)
            {
                // Stop vertex polygon on right-click
                var _ = FinishPolygonAsync();
            }
        }

        private void MapView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine($"Mouse double click called with geometry too  = {_geometryEditor.Tool.GetType().Name}, IsStarted = {_geometryEditor.IsStarted}");

            if (_geometryEditor.IsStarted && _geometryEditor.Tool is VertexTool)
            {
                // Stop vertex polygon on double-click
                var _ = FinishPolygonAsync();
            }
        }

        #endregion

        #region Start drawing methods

        private void StartVertexPolygon()
        {
            Debug.WriteLine($"Start vertex polygon called with IsStarted = {_geometryEditor.IsStarted}");
            if (_geometryEditor.IsStarted)
                _geometryEditor.Stop();

            MapPoint mapPoint = _mapView.ScreenToLocation(_startPoint)!;

            // Create a polygon with a single vertex (or empty ring)
            var builder = new PolygonBuilder(_mapView.SpatialReference);
            builder.AddPoint(mapPoint);

            _geometryEditor.Tool = new VertexTool()
            {
                Style = {
                    VertexSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 8),
                    MidVertexSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Square, System.Drawing.Color.Orange, 4),
                    LineSymbol = _lineStyle,
                    FillSymbol = _fillSymbol
                }
            };
            _geometryEditor.Start(builder.ToGeometry());

            Debug.WriteLine($"Started vertex polygon called with IsStarted = {_geometryEditor.IsStarted}");
        }

        private void StartFreehandLasso()
        {
            Debug.WriteLine($"Start freehand polygon called with IsStarted = {_geometryEditor.IsStarted}");

            if (_geometryEditor.IsStarted)
                _geometryEditor.Stop();

            _geometryEditor.Tool = new FreehandTool()
            {
                Style = { LineSymbol = _lineStyle, FillSymbol = _fillSymbol }
            };
            _geometryEditor.Start(GeometryType.Polygon);

            Debug.WriteLine($"Started vertex polygon called with IsStarted = {_geometryEditor.IsStarted}");
        }

        #endregion

        #region GeometryEditor PropertyChanged

        private async void GeometryEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Geometry PropertyChanged called with IsStarted = {_geometryEditor.IsStarted}, Property : {e.PropertyName}");


            if (e.PropertyName == nameof(GeometryEditor.IsStarted) && !_geometryEditor.IsStarted)
            {
                var _ = FinishPolygonAsync();
            }
        }

        private async Task FinishPolygonAsync()
        {
            Debug.WriteLine($"Finish polygon called with IsStarted = {_geometryEditor.IsStarted}");

            if (!_geometryEditor.IsStarted) return;

            var polygon = _geometryEditor.Stop() as Polygon;
            if (polygon != null)
            {
                await SelectFeaturesUnderPolygonAsync(polygon);
            }
            CleanUp();
        }

        #endregion

        #region Feature selection

        private async Task SelectFeaturesUnderPolygonAsync(Polygon polygon)
        {
            Debug.WriteLine($"Select feature called with [polygon == null] = {polygon == null}");

            if (polygon == null) return;

            foreach (var layer in _mapView.Map.OperationalLayers.OfType<FeatureLayer>())
            {
                if (!layer.IsVisible) continue;

                var query = new QueryParameters
                {
                    Geometry = polygon,
                    SpatialRelationship = SpatialRelationship.Intersects
                };

                layer.ClearSelection();
                await layer.SelectFeaturesAsync(query, SelectionMode.New);
            }
        }

        #endregion
    }
}
