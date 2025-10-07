using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using KGWin.WPF.Interfaces;
using KGWin.WPF.Models;
using KGWin.WPF.ViewModels.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace KGWin.WPF.ViewModels
{
    /// <summary>
    /// Provides map data to an application
    /// </summary>
    public class KGMapViewModel : ViewModelBase
    {
        public KGMapViewModel(
                IConfiguration configuration,
                IArcGisService arcGisService,
                KGPopupViewModel popupViewModel
            )
        {
            _configuration = configuration;
            _arcGisService = arcGisService;

            _mapConfig = MapConfig.GetConfig(LocationName.Default, _configuration);

            var _ = InitializeAsync(_mapConfig);
            _kgPopupViewModel = popupViewModel;
            //_kgPopupViewModel.BodyData = PopupRows;
        }

        private IConfiguration _configuration;
        private IArcGisService _arcGisService;
        private Map _map = default!;
        private MapConfig _mapConfig;
        private ObservableCollection<KGLayerItemViewModel> _layers = [];
        private ObservableCollection<KGLabelValueViewModel> _popupRows = [];
        private KGPopupViewModel _kgPopupViewModel;

        public Map Map
        {
            get => _map;
            set => SetProperty(ref _map, value);
        }

        public MapConfig MapConfig
        {
            get => _mapConfig;
            set => SetProperty(ref _mapConfig, value);
        }

        public ObservableCollection<KGLayerItemViewModel> Layers
        {
            get => _layers;
            set => SetProperty(ref _layers, value);
        }

        public ObservableCollection<KGLabelValueViewModel> PopupRows
        {
            get => _popupRows;
            set => SetProperty(ref _popupRows, value);
        }

        public KGPopupViewModel KGPopupViewModel
        {
            get => _kgPopupViewModel;
            set => SetProperty(ref _kgPopupViewModel, value);
        }

        public async Task InitializeAsync(MapConfig config)
        {
            MapConfig = config;

            if (string.IsNullOrWhiteSpace(MapConfig.VtpkPath))
            {
                Map = new Map(SpatialReferences.WebMercator)
                {
                    InitialViewpoint = config.Viewpoint,
                    Basemap = new Basemap(BasemapStyle.ArcGISStreets)
                };
            }
            else
            {
                if (MapConfig.LoadLayers)
                {
                    await LoadMapAndLayersAsync();
                }
                else
                {
                    await LoadVtpkAsync();
                }
            }
        }

        public async Task HandleMapClick(MapView mapView, GeoViewInputEventArgs e)
        {
            // Close any existing popup
            KGPopupViewModel.ClosePopup();
            KGPopupViewModel.Buttons.Clear();
            PopupRows.Clear();

            var fields = await _arcGisService.ExtractMapObjectDataOnClickPoint(mapView, e.Position, KGPopupViewModel);
            var fieldsKeys = fields.Keys.ToList();

            fieldsKeys.ForEach(key =>
            {
                var labelValueVM = App.Services.GetRequiredService<KGLabelValueViewModel>();
                labelValueVM.Label = key;
                labelValueVM.Value = fields[key];
                PopupRows.Add(labelValueVM);                
            });

            KGPopupViewModel.BodyData = PopupRows;

            List<string> buttonConents = new()
            {
                "Create Work Order",
                "Create Work Request",
                "Update Asset Status",
                "Add to Work Order"
            };

            buttonConents.ForEach(content =>
            {
                var buttonVM = App.Services.GetRequiredService<KGButtonViewModel>();

                Action command = content == buttonConents[0]
                    ? CreateWorkOrder
                    : () => { };

                buttonVM.ButtonContent = content;
                buttonVM.ButtonCommand = new RelayCommand(command);

                KGPopupViewModel.Buttons.Add(buttonVM);
            });

        }

        private async Task LoadMapAndLayersAsync()
        {
            if (File.Exists(MapConfig.MmpkPath))
            {
                var mmpk = await MobileMapPackage.OpenAsync(MapConfig.MmpkPath);

                if (mmpk.Maps.Count > 0)
                {
                    var mobileMap = mmpk.Maps[0];

                    await LoadVtpkAsync(mobileMap);
                    mobileMap.InitialViewpoint = MapConfig.Viewpoint;

                    Map = mobileMap;

                    if (Map.OperationalLayers != null)
                    {
                        foreach (var layer in Map.OperationalLayers)
                        {
                            var layerItem = _arcGisService.ExtractLayerMetadata(layer);

                            layerItem.PropertyChanged += (s, e) =>
                            {
                                if (e.PropertyName == nameof(KGLayerItemViewModel.IsVisible))
                                {
                                    layer.IsVisible = layerItem.IsVisible;
                                }
                            };

                            Layers.Add(layerItem);
                        }
                    }
                }
            }
        }

        private async Task LoadVtpkAsync(Map? map = null)
        {
            if (File.Exists(MapConfig.VtpkPath))
            {
                Uri vtpkUri = new(MapConfig.VtpkPath);
                ArcGISVectorTiledLayer vectorTileLayer = new(vtpkUri);
                await vectorTileLayer.LoadAsync();

                if (MapConfig.LoadLayers && map != null)
                {
                    map.Basemap = new Basemap(vectorTileLayer);
                }
                else
                {
                    Map = new Map(SpatialReferences.WebMercator)
                    {
                        Basemap = new Basemap(vectorTileLayer),
                        InitialViewpoint = MapConfig.Viewpoint
                    };
                }

            }
        }

        private void CreateWorkOrder()
        {
            MessageBox.Show("Hello");

            //try
            //{
            //    // Minimize the hosting modal popup window (instead of closing)
            //    try
            //    {
            //        var currentWindow = System.Windows.Application.Current.Windows.OfType<Views.ModalPopupWindow>()
            //            .FirstOrDefault(w => w.PopupContent is Views.NapervilleView);
            //        if (currentWindow != null)
            //        {
            //            currentWindow.WindowState = System.Windows.WindowState.Minimized;
            //        }
            //    }
            //    catch { }

            //    // Navigate main app to Web page
            //    if (System.Windows.Application.Current.MainWindow?.DataContext is MainViewModel mainVM)
            //    {
            //        // Ensure WebView has an address
            //        if (mainVM.WebViewModel != null && string.IsNullOrWhiteSpace(mainVM.WebViewModel.BrowserAddress))
            //        {
            //            mainVM.WebViewModel.BrowserAddress = "http://localhost:4200/";
            //        }

            //        mainVM.NavigateToWebCommand.Execute(null);

            //        // After navigation, ask Angular to switch to Create Work Order page
            //        // Send a command payload the web app listens for
            //        // Build fields collection from current popup rows
            //        var fields = new System.Collections.Generic.List<object>();
            //        foreach (var r in PopupRows)
            //        {
            //            fields.Add(new { label = r.Label, value = r.Value });
            //        }

            //        var payload = new { command = "switchToCreateWorkOrder", context = new { title = PopupTitle, data = SelectedFeatureData, fields } };
            //        var json = System.Text.Json.JsonSerializer.Serialize(payload);

            //        // 1) CefSharp path (existing logic via CommunicationService in CSWebView)
            //        mainVM.SendMessageToAngularViaWebView(json);

            //        // 2) WebView2 path (if available)
            //        try
            //        {
            //            if (mainVM.WebView2 != null)
            //            {
            //                // Wrap into the same shape Angular listens to: window.postMessage({ type, data }, '*')
            //                var messageJson = System.Text.Json.JsonSerializer.Serialize(new { type = "communication", data = json });
            //                mainVM.WebView2.SendMessageToAngular(messageJson);
            //            }
            //        }
            //        catch { }

            //        System.Diagnostics.Debug.WriteLine("Sent navigate command to Angular: " + json);
            //    }

            //    // Also clear/close the in-view popup content if showing
            //    ClosePopup();
            //}
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine($"Error creating work order: {ex.Message}");
            //    System.Windows.MessageBox.Show(
            //        $"Error creating work order: {ex.Message}",
            //        "Error",
            //        System.Windows.MessageBoxButton.OK,
            //        System.Windows.MessageBoxImage.Error);
            //}
        }

    }
}
