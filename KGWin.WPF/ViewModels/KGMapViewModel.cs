using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using KGWin.WPF.Interfaces;
using KGWin.WPF.Models;
using KGWin.WPF.ViewModels.Base;
using KGWin.WPF.Views.Components;
using KGWin.WPF.Views.Map;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

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
                IMapDrawService drawService,
                EventAggregator eventAggregator,
                KGPopupViewModel mapObjectMetaPopupViewModel,
                KGMapControlToolbarViewModel toolbarViewModel
            )
        {
            _configuration = configuration;
            _arcGisService = arcGisService;
            _drawService = drawService;

            _mapObjectMetaPopupViewModel = mapObjectMetaPopupViewModel;

            EventAggregator = eventAggregator;
            _toolbarViewModel = toolbarViewModel;
            _toolbarViewModel.EventAggregator = EventAggregator;

            EventAggregator.Subscribe<KGEvent>(HandleEvents);
        }

        private IConfiguration _configuration;
        private IArcGisService _arcGisService;
        private IMapDrawService _drawService;
        public readonly EventAggregator EventAggregator;

        private Map _map = default!;
        private MapConfig _mapConfig;
        private ObservableCollection<KGLayerItemViewModel> _layers = [];
        private ObservableCollection<KGLabelValueViewModel> _popupRows = [];
        private KGMapControlToolbarViewModel _toolbarViewModel;
        private KGPopupViewModel _mapObjectMetaPopupViewModel;

        public MapView KGMapView { get; set; }

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

        public KGMapControlToolbarViewModel ToolbarViewModel
        {
            get => _toolbarViewModel;
            set => SetProperty(ref _toolbarViewModel, value);
        }

        public KGPopupViewModel MapObjectMetaPopupViewModel
        {
            get => _mapObjectMetaPopupViewModel;
            set => SetProperty(ref _mapObjectMetaPopupViewModel, value);
        }

        public ICommand GeoViewTappedCommand { get; private set; }

        public void AssiginView(MapView mapView)
        {
            KGMapView = mapView;
            _drawService.Initialize(KGMapView);
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

        public async Task RefreshViewpoint()
        {
            if (KGMapView != null)
            {
                var currentViewpoint = KGMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

                if (currentViewpoint != null)
                {
                    await KGMapView.SetViewpointAsync(currentViewpoint);
                }
            }
        }

        private void HandleEvents(KGEvent kgEvent)
        {
            switch (kgEvent.EventName)
            {
                case nameof(MapView.GeoViewTapped):
                    {
                        if (_drawService.IsDrawingModeOn) return;

                        var _ = HandleMapClick((MapView)kgEvent.Sender!, (GeoViewInputEventArgs)kgEvent.Args);
                        break;
                    }
                case KGMapControlToolbar.LassoButtonClick:
                    {
                        _drawService.OnLassoSelectClick(kgEvent.Sender!, (RoutedEventArgs)kgEvent.Args);
                        break;
                    }
            }
        }

        public async Task HandleMapClick(MapView mapView, GeoViewInputEventArgs e)
        {
            // Close any existing popup
            MapObjectMetaPopupViewModel.ClosePopup();
            MapObjectMetaPopupViewModel.Buttons.Clear();
            PopupRows.Clear();

            var fields = await _arcGisService.ExtractMapObjectDataOnClickPoint(mapView, e.Position, MapObjectMetaPopupViewModel);
            var fieldsKeys = fields.Keys.ToList();

            fieldsKeys.ForEach(key =>
            {
                var labelValueVM = App.Services.GetRequiredService<KGLabelValueViewModel>();
                labelValueVM.Label = key;
                labelValueVM.Value = fields[key];
                PopupRows.Add(labelValueVM);
            });

            MapObjectMetaPopupViewModel.BodyData = PopupRows;

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

                MapObjectMetaPopupViewModel.Buttons.Add(buttonVM);
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
                  
                    if (mobileMap.OperationalLayers != null)
                    {
                        var tasks = mobileMap
                            .OperationalLayers
                            .Select(l => l.LoadAsync());

                        await Task.WhenAll(tasks);
                    }

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
                                    Application.Current.Dispatcher.Invoke(() => layer.IsVisible = layerItem.IsVisible);

                                    //layer.IsVisible = layerItem.IsVisible;
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
            var currentWindow = Application.Current.Windows
                .OfType<KGModalPopupWindow>()
                .FirstOrDefault(w => ((KGModalPopupWindowViewModel)w.DataContext).PopupContent is KGMap);

            if (currentWindow != null)
            {
                currentWindow.WindowState = System.Windows.WindowState.Minimized;
            }

            MessageBox.Show("Switch to create work order in progress");
        }

    }
}
