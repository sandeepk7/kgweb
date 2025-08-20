using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;

namespace KGWin
{
    /// <summary>
    /// Interaction logic for MapPage.xaml
    /// </summary>
    public partial class MapPage : Page, INotifyPropertyChanged
    {
        private Map? _map;
        private Map? _offlineMap;
        private GraphicsOverlay? _graphicsOverlay;
        private int _pointCounter = 0;
        private const string ServerUrl = "https://www.arcgis.com/sharing/rest";
        private const string WebMapParisId = "e5039444ef3c48b8a8fdc9227f9be7c1";
        private const string WebMapLAId = "c585c467996b4e4fa59decf10ccb47f1";

        public Map? Map
        {
            get => _map;
            set
            {
                _map = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MapPage()
        {
            InitializeComponent();
            DataContext = this;
            InitializeMap();
            
            // Subscribe to map view events
            MyMapView.ViewpointChanged += MyMapView_ViewpointChanged;
            OfflineMapView.ViewpointChanged += OfflineMapView_ViewpointChanged;
            
            // Initialize graphics overlay for asset points
            _graphicsOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_graphicsOverlay);
            
            // Subscribe to mouse events for asset interaction
            MyMapView.MouseLeftButtonDown += MyMapView_MouseLeftButtonDown;
        }

        private async void InitializeMap()
        {
            try
            {
                ArcGISPortal arcgisPortal = await ArcGISPortal.CreateAsync(new Uri(ServerUrl));
                PortalItem portalItem = await PortalItem.CreateAsync(arcgisPortal, WebMapLAId);
                Map = new Map(portalItem);

                MapOverlayText.Background = System.Windows.Media.Brushes.Green;
                MapOverlayText.Foreground = System.Windows.Media.Brushes.White;

                ////// Create a graphics overlay for custom points
                //_graphicsOverlay = new GraphicsOverlay();
                //MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing map: {ex.Message}", "Map Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LiveMapTab_Click(object sender, RoutedEventArgs e)
        {
            // Switch to Live Map tab
            LiveMapContent.Visibility = Visibility.Visible;
            OfflineMapContent.Visibility = Visibility.Collapsed;
            
            // Update tab button styles
            LiveMapTab.Style = (Style)FindResource("ActiveTabButtonStyle");
            OfflineMapTab.Style = (Style)FindResource("TabButtonStyle");
        }

        private void OfflineMapTab_Click(object sender, RoutedEventArgs e)
        {
            // Switch to Offline Map tab
            LiveMapContent.Visibility = Visibility.Collapsed;
            OfflineMapContent.Visibility = Visibility.Visible;
            
            // Update tab button styles
            LiveMapTab.Style = (Style)FindResource("TabButtonStyle");
            OfflineMapTab.Style = (Style)FindResource("ActiveTabButtonStyle");
        }

        private async void ShowAssetLayer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear existing graphics
                _graphicsOverlay?.Graphics.Clear();

                // Create highlight yellow symbol for all assets
                var highlightYellowSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Yellow, 12);

                // Create 3 asset points in LA County (approximately 20 miles apart)
                // LA County coordinates: roughly 34.0522° N, 118.2437° W
                
                // Asset 1: Downtown LA
                var asset1Point = new MapPoint(-118.2437, 34.0522, SpatialReferences.Wgs84);
                var asset1Graphic = new Graphic(asset1Point, highlightYellowSymbol);
                asset1Graphic.Attributes["AssetID"] = "LA001";
                asset1Graphic.Attributes["AssetName"] = "Downtown LA Asset";
                asset1Graphic.Attributes["AssetType"] = "Infrastructure";
                asset1Graphic.Attributes["Description"] = "Major infrastructure hub in downtown Los Angeles";
                _graphicsOverlay?.Graphics.Add(asset1Graphic);

                // Asset 2: Santa Monica (about 20 miles west)
                var asset2Point = new MapPoint(-118.4912, 34.0195, SpatialReferences.Wgs84);
                var asset2Graphic = new Graphic(asset2Point, highlightYellowSymbol);
                asset2Graphic.Attributes["AssetID"] = "LA002";
                asset2Graphic.Attributes["AssetName"] = "Santa Monica Asset";
                asset2Graphic.Attributes["AssetType"] = "Transportation";
                asset2Graphic.Attributes["Description"] = "Transportation center near Santa Monica Pier";
                _graphicsOverlay?.Graphics.Add(asset2Graphic);

                // Asset 3: Pasadena (about 20 miles northeast)
                var asset3Point = new MapPoint(-118.1445, 34.1478, SpatialReferences.Wgs84);
                var asset3Graphic = new Graphic(asset3Point, highlightYellowSymbol);
                asset3Graphic.Attributes["AssetID"] = "LA003";
                asset3Graphic.Attributes["AssetName"] = "Pasadena Asset";
                asset3Graphic.Attributes["AssetType"] = "Utility";
                asset3Graphic.Attributes["Description"] = "Utility infrastructure in Pasadena area";
                _graphicsOverlay?.Graphics.Add(asset3Graphic);

                // Update overlay text to show asset layer is active
                MapOverlayText.Text = "ESRI Live Map View - Asset Layer Active";
                MapOverlayText.Background = System.Windows.Media.Brushes.DarkGreen;
                MapOverlayText.Foreground = System.Windows.Media.Brushes.White;

                // Show simple success message without details
                MessageBox.Show("Asset layer has been added successfully. Click on the yellow markers to see asset details.", 
                              "Asset Layer Added", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding asset layer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            var helpMessage = @"To use ESRI ArcGIS map services, you need to create a free developer account and get an API key:

1. Go to https://developers.arcgis.com/
2. Click 'Sign Up' to create a free account
3. Once registered, go to your dashboard
4. Create a new project or use an existing one
5. Go to 'API Keys' section in your project
6. Create a new API key with appropriate permissions
7. Copy the API key and add it to the 'appsettings.json' file

The free tier includes:
• 20,000 map tile requests per month
• Access to ArcGIS Online services
• Basic geocoding and routing

Would you like to open the ESRI Developer website?";

            var result = MessageBox.Show(helpMessage, "Get ESRI Credentials", MessageBoxButton.YesNo, MessageBoxImage.Information);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://developers.arcgis.com/",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open website: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Determine which map view is currently active
                Esri.ArcGISRuntime.UI.Controls.MapView activeMapView = GetActiveMapView();
                if (activeMapView != null)
                {
                    // Get current viewpoint and zoom in
                    var currentViewpoint = activeMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
                    if (currentViewpoint != null)
                    {
                        // Calculate new scale for zooming in (smaller scale = more zoomed in)
                        var currentScale = activeMapView.MapScale;
                        var newScale = currentScale * 0.5; // Zoom in by factor of 2
                        
                        // Create new viewpoint with the same center but new scale
                        var newViewpoint = new Viewpoint(currentViewpoint.TargetGeometry, newScale);
                        activeMapView.SetViewpointAsync(newViewpoint, TimeSpan.FromSeconds(1));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error zooming in: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Determine which map view is currently active
                Esri.ArcGISRuntime.UI.Controls.MapView activeMapView = GetActiveMapView();
                if (activeMapView != null)
                {
                    // Get current viewpoint and zoom out
                    var currentViewpoint = activeMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
                    if (currentViewpoint != null)
                    {
                        // Calculate new scale for zooming out (larger scale = more zoomed out)
                        var currentScale = activeMapView.MapScale;
                        var newScale = currentScale * 2.0; // Zoom out by factor of 2
                        
                        // Create new viewpoint with the same center but new scale
                        var newViewpoint = new Viewpoint(currentViewpoint.TargetGeometry, newScale);
                        activeMapView.SetViewpointAsync(newViewpoint, TimeSpan.FromSeconds(1));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error zooming out: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ResetView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Determine which map view is currently active
                Esri.ArcGISRuntime.UI.Controls.MapView activeMapView = GetActiveMapView();
                if (activeMapView == MyMapView)
                {
                    // Reset live map
                    ArcGISPortal arcgisPortal = await ArcGISPortal.CreateAsync(new Uri(ServerUrl));
                    PortalItem portalItem = await PortalItem.CreateAsync(arcgisPortal, WebMapLAId);
                    Map = new Map(portalItem);
                    MyMapView.Map = Map;
                }
                else if (activeMapView == OfflineMapView && _offlineMap != null)
                {
                    // Reset offline map to its original state
                    OfflineMapView.Map = _offlineMap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resetting view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Esri.ArcGISRuntime.UI.Controls.MapView GetActiveMapView()
        {
            if (LiveMapContent.Visibility == Visibility.Visible)
            {
                return MyMapView;
            }
            else
            {
                return OfflineMapView;
            }
        }

        private async void LoadVTPK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Path to your .vtpk file in the Data folder
                string vtpkPath = @"D:\Work\Office\Damco\KloudGin\POC\KGWin\KGWin\Data\Naperville.vtpk";

                // Create a new ArcGISVectorTiledLayer from the VTPK file
                var vectorTiledLayer = new ArcGISVectorTiledLayer(new Uri(vtpkPath));
                await vectorTiledLayer.LoadAsync();

                // Create a new basemap using the loaded vector tiled layer
                var basemap = new Basemap(vectorTiledLayer);

                // Create a new map with the basemap
                _offlineMap = new Map(basemap);

                // Assign the map with the VTPK basemap to the Offline MapView
                OfflineMapView.Map = _offlineMap;

                // Update overlay text
                OfflineMapOverlayText.Text = "ESRI VTPK Map View";
                OfflineMapOverlayText.Background = System.Windows.Media.Brushes.Orange;
                OfflineMapOverlayText.Foreground = System.Windows.Media.Brushes.White;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading VTPK basemap: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadMMPK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string vtpkPath = @"D:\Work\Office\Damco\KloudGin\POC\KGWin\KGWin\Data\Naperville.vtpk";
                var vectorTiledLayer = new ArcGISVectorTiledLayer(new Uri(vtpkPath));
                await vectorTiledLayer.LoadAsync();

                var basemap = new Basemap(vectorTiledLayer);
                _offlineMap = new Map(basemap);

                string mmpkPath = @"D:\Work\Office\Damco\KloudGin\POC\KGWin\KGWin\Data\NapervilleWaterUtility.mmpk";
                var mmpk = new MobileMapPackage(mmpkPath);
                await mmpk.LoadAsync();

                if (mmpk.Maps.Count > 0)
                {
                    foreach (var layer in mmpk.Maps[0].OperationalLayers)
                    {
                        // Clone the layer before adding
                        var clonedLayer = layer.Clone();
                        _offlineMap.OperationalLayers.Add(clonedLayer);
                    }

                    OfflineMapView.Map = _offlineMap;

                    // Update overlay text
                    OfflineMapOverlayText.Text = "ESRI VTPK + MMPK Map View";
                    OfflineMapOverlayText.Background = System.Windows.Media.Brushes.Purple;
                    OfflineMapOverlayText.Foreground = System.Windows.Media.Brushes.White;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading MMPK: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MyMapView_ViewpointChanged(object? sender, EventArgs e)
        {
            try
            {
                // Update the map information panel with current location and zoom
                var viewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale);
                if (viewpoint?.TargetGeometry is MapPoint centerPoint)
                {
                    // Update latitude and longitude
                    LatitudeText.Text = $"Latitude: {centerPoint.Y:F4}°";
                    LongitudeText.Text = $"Longitude: {centerPoint.X:F4}°";
                    
                    // Update zoom level (scale)
                    var scale = viewpoint.TargetScale;
                    var zoomLevel = Math.Round(Math.Log(591657550.5 / scale, 2));
                    ZoomLevelText.Text = $"Zoom Level: {zoomLevel:F0}";
                }
            }
            catch (Exception ex)
            {
                // Silently handle errors in the viewpoint changed event
                System.Diagnostics.Debug.WriteLine($"Error updating map info: {ex.Message}");
            }
        }

        private void OfflineMapView_ViewpointChanged(object? sender, EventArgs e)
        {
            try
            {
                // Update the offline map information panel with current location and zoom
                var viewpoint = OfflineMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale);
                if (viewpoint?.TargetGeometry is MapPoint centerPoint)
                {
                    // Update latitude and longitude
                    OfflineLatitudeText.Text = $"Latitude: {centerPoint.Y:F4}°";
                    OfflineLongitudeText.Text = $"Longitude: {centerPoint.X:F4}°";
                    
                    // Update zoom level (scale)
                    var scale = viewpoint.TargetScale;
                    var zoomLevel = Math.Round(Math.Log(591657550.5 / scale, 2));
                    OfflineZoomLevelText.Text = $"Zoom Level: {zoomLevel:F0}";
                }
            }
            catch (Exception ex)
            {
                // Silently handle errors in the viewpoint changed event
                System.Diagnostics.Debug.WriteLine($"Error updating offline map info: {ex.Message}");
            }
        }

        private async void ShowAssetDetails_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // For now, we'll show all asset details in a single message box
                // In a production app, you might want to implement proper tooltip functionality
                var assetDetails = "Asset Layer Details:\n\n" +
                                 "Asset 1: LA001 - Downtown LA Asset (Infrastructure)\n" +
                                 "Description: Major infrastructure hub in downtown Los Angeles\n\n" +
                                 "Asset 2: LA002 - Santa Monica Asset (Transportation)\n" +
                                 "Description: Transportation center near Santa Monica Pier\n\n" +
                                 "Asset 3: LA003 - Pasadena Asset (Utility)\n" +
                                 "Description: Utility infrastructure in Pasadena area";
                
                MessageBox.Show(assetDetails, "Asset Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing asset details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MyMapView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Only process if we have graphics to check
                if (_graphicsOverlay?.Graphics.Count == 0)
                {
                    return;
                }

                // Get the screen position
                var screenPoint = e.GetPosition(MyMapView);
                
                // Use IdentifyGraphicsOverlayAsync to check if we clicked on any graphics
                var identifyResult = await MyMapView.IdentifyGraphicsOverlayAsync(_graphicsOverlay, screenPoint, 20, false);
                
                if (identifyResult.Graphics.Count > 0)
                {
                    // We found a graphic, get the first one
                    var graphic = identifyResult.Graphics.First();
                    
                    // Get asset information from the graphic attributes
                    var assetId = graphic.Attributes["AssetID"]?.ToString() ?? "Unknown";
                    var assetName = graphic.Attributes["AssetName"]?.ToString() ?? "Unknown Asset";
                    var assetType = graphic.Attributes["AssetType"]?.ToString() ?? "Unknown Type";
                    var description = graphic.Attributes["Description"]?.ToString() ?? "No description available";
                    
                    // Create and show the custom popup
                    var popup = new AssetPopup();
                    popup.SetAssetInformation(assetId, assetName, assetType, description);
                    
                    // Position the popup near the click location
                    var mapViewPoint = MyMapView.PointToScreen(screenPoint);
                    popup.SetPosition(mapViewPoint.X + 10, mapViewPoint.Y - 150);
                    
                    // Show the popup
                    popup.Show();
                }
            }
            catch (Exception ex)
            {
                // Silently handle errors in the mouse click event
                System.Diagnostics.Debug.WriteLine($"Error handling mouse click: {ex.Message}");
            }
        }



        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
