using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using KGWin.WPF.Interfaces;
using KGWin.WPF.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Windows;
using System.Windows.Input;


namespace KGWin.WPF.Services
{
    public class ArcGisService : IArcGisService
    {
        public ArcGisService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly IConfiguration _configuration;

        public void Initialize()
        {
            /* Authentication for ArcGIS location services:
             * Use of ArcGIS location services, including basemaps and geocoding, requires either:
             * 1) User authentication: Automatically generates a unique, short-lived access token when a user signs in to your application with their ArcGIS account
             *    giving your application permission to access the content and location services authorized to an existing ArcGIS user's account.
             * 2) API key authentication: Uses a long-lived access token to authenticate requests to location services and private content.
             *    Go to https://links.esri.com/create-an-api-key to learn how to create and manage an API key using API key credentials, and then call 
             *    .UseApiKey("Your ArcGIS location services API Key")
             *    in the initialize call below. */

            /* Licensing:
             * Production deployment of applications built with the ArcGIS Maps SDK requires you to license ArcGIS functionality.
             * For more information see https://links.esri.com/arcgis-runtime-license-and-deploy.
             * You can set the license string by calling .UseLicense(licenseString) in the initialize call below 
             * or retrieve a license dynamically after signing into a portal:
             * ArcGISRuntimeEnvironment.SetLicense(await myArcGISPortal.GetLicenseInfoAsync()); */
            try
            {
                string apiKey = _configuration["ArcGIS:ApiKey"]!;
                // Initialize the ArcGIS Maps SDK runtime before any components are created.
                ArcGISRuntimeEnvironment.Initialize(config => config
                // .UseLicense("[Your ArcGIS Maps SDK license string]")
                 .UseApiKey(apiKey)
                  .ConfigureAuthentication(auth => auth
                     .UseDefaultChallengeHandler() // Use the default authentication dialog
                  // .UseOAuthAuthorizeHandler(myOauthAuthorizationHandler) // Configure a custom OAuth dialog
                   )
                );
                // Enable support for TimestampOffset fields, which also changes behavior of Date fields.
                // For more information see https://links.esri.com/DotNetDateTime
                ArcGISRuntimeEnvironment.EnableTimestampOffsetSupport = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ArcGIS Maps SDK runtime initialization failed.");
            }
        }

        public KGLayerItemViewModel ExtractLayerMetadata(Layer layer)
        {
            var layerItem = new KGLayerItemViewModel
            {
                Name = layer.Name ?? "Unnamed Layer",
                LayerType = layer.GetType().Name,
                Layer = layer,
                IsVisible = layer.IsVisible
            };

            // Extract basic metadata
            layerItem.Description = layer.Description ?? "No description available";

            // Extract extent information
            if (layer.FullExtent != null)
            {
                var extent = layer.FullExtent;
                layerItem.FullExtent = $"X: {extent.XMin:F2} to {extent.XMax:F2}, Y: {extent.YMin:F2} to {extent.YMax:F2}";
            }
            else
            {
                layerItem.FullExtent = "No extent information";
            }

            // Extract scale information
            layerItem.MinScale = layer.MinScale.ToString("N0");
            layerItem.MaxScale = layer.MaxScale.ToString("N0");

            // Try to extract additional metadata based on layer type
            if (layer is ArcGISMapImageLayer mapImageLayer)
            {
                layerItem.Url = mapImageLayer.Source?.ToString() ?? "No URL available";
                layerItem.ItemId = mapImageLayer.Item?.ItemId ?? "No Item ID";
            }
            else if (layer is ArcGISTiledLayer tiledLayer)
            {
                layerItem.Url = tiledLayer.Source?.ToString() ?? "No URL available";
                layerItem.ItemId = tiledLayer.Item?.ItemId ?? "No Item ID";
            }
            else
            {
                layerItem.Url = "Offline layer - no URL";
                layerItem.ItemId = "Offline layer - no Item ID";
            }

            return layerItem;
        }

        public async Task<Dictionary<string, string>> ExtractMapObjectDataOnClickPoint(MapView mapView, Point position, KGPopupViewModel kgPopupVM)
        {
            Dictionary<string, string> fields = [];
            // Set up identify parameters
            double tolerance = 10; // pixels
            bool returnPopupsOnly = false;
            int maxResults = 10;

            // Identify features at the clicked location
            var identifyResult = await mapView.IdentifyLayersAsync(position, tolerance, returnPopupsOnly, maxResults);

            var layerResult = identifyResult?.FirstOrDefault();

            // Find the first layer with results
            if (layerResult?.GeoElements != null && layerResult.GeoElements.Count > 0)
            {
                // Get the layer name
                var layerName = layerResult.LayerContent?.Name ?? "Unknown Layer";

                // Get the first feature
                var feature = layerResult.GeoElements[0];

                // Extract attributes
                var attributes = feature.Attributes;

                if (attributes != null && attributes.Count > 0)
                {
                    // Get Object ID from attributes
                    var objectId = "Unknown";
                    if (attributes.ContainsKey("OBJECTID"))
                    {
                        objectId = attributes["OBJECTID"]?.ToString() ?? "Unknown";
                    }
                    else if (attributes.ContainsKey("ObjectId"))
                    {
                        objectId = attributes["ObjectId"]?.ToString() ?? "Unknown";
                    }
                    else if (attributes.ContainsKey("FID"))
                    {
                        objectId = attributes["FID"]?.ToString() ?? "Unknown";
                    }

                    if (layerResult.LayerContent is FeatureLayer featureLayer && featureLayer.PopupDefinition != null)
                    {
                        var pd = featureLayer.PopupDefinition;
                        if (pd.Fields != null && pd.Fields.Count > 0)
                        {
                            foreach (var pf in pd.Fields)
                            {
                                var fieldName = pf.FieldName;
                                var label = string.IsNullOrWhiteSpace(pf.Label) ? fieldName : pf.Label;
                                string val = attributes.ContainsKey(fieldName) ? attributes[fieldName]?.ToString() ?? "-" : "-";
                                fields[label] = val;
                            }
                        }
                    }

                    // Position near click with clamping to visible area of the MapView
                    double desiredX = position.X + 10; // right of click
                    double desiredY = position.Y - 10; // above click

                    // Try to get the map view size to clamp within bounds
                    var mapViewRef = mapView;
                    double viewWidth = mapViewRef?.ActualWidth ?? 0;
                    double viewHeight = mapViewRef?.ActualHeight ?? 0;

                    // Estimated popup size bounds (match XAML MaxWidth/MaxHeight)
                    double popupWidth = kgPopupVM.Width; // MaxWidth
                    double popupHeight = kgPopupVM.Height; // MaxHeight

                    // Clamp horizontally
                    if (viewWidth > 0)
                    {
                        if (desiredX + popupWidth > viewWidth) desiredX = Math.Max(0, viewWidth - popupWidth - 10);
                    }

                    // Clamp vertically (ensure fully visible)
                    if (viewHeight > 0)
                    {
                        if (desiredY + popupHeight > viewHeight) desiredY = Math.Max(0, viewHeight - popupHeight - 10);
                        if (desiredY < 0) desiredY = 10;
                    }

                    kgPopupVM.X = desiredX;
                    kgPopupVM.Y = desiredY;

                    // Show popup with Layer Name + Object ID as title
                    kgPopupVM.Title = $"{layerName} - ID: {objectId}";
                    kgPopupVM.IsVisible = true;
                }
            }

            return fields;
        }
    }
}
