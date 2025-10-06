using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using KGWin.WPF.Constants;
using KGWin.WPF.Services;
using Microsoft.Extensions.Configuration;

namespace KGWin.WPF.Models
{
    public class MapConfig
    {
        public LocationName LocationName { get; set; }
        public string VtpkPath { get; set; } = string.Empty;
        public string MmpkPath { get; set; } = string.Empty;
        public Viewpoint Viewpoint { get; set; } = default!;
        public bool LoadLayers { get; set; } = false;

        public static MapConfig GetConfig(LocationName? locationName = null, IConfiguration? configuration = null)
        {
            if (locationName != null && configuration != null)
            {
                switch (locationName)
                {
                    case LocationName.Default:
                        //{
                        //    double x = double.Parse(configuration["MapPopup:Default:InitialX"] ?? MapConstants.DefaultX.ToString());
                        //    double y = double.Parse(configuration["MapPopup:Default:InitialY"] ?? MapConstants.DefaultY.ToString());
                        //    var scale = int.Parse(configuration["MapPopup:Default:InitialScale"] ?? MapConstants.DefaultScale.ToString());
                        //    MapPoint mapPoint = new(x, y, SpatialReferences.Wgs84);
                        //    Viewpoint viewpoint = new(mapPoint, scale);

                        //    MapConfig config = new()
                        //    {
                        //        LoadLayers = false,
                        //        LocationName = LocationName.Default,
                        //        MmpkPath = string.Empty,
                        //        VtpkPath = string.Empty,
                        //        Viewpoint = viewpoint
                        //    };
                        //    return config;
                        //}
                    case LocationName.Naperville:
                        {
                            double x = double.Parse(configuration["MapPopup:Naperville:InitialX"] ?? MapConstants.DefaultX.ToString());
                            double y = double.Parse(configuration["MapPopup:Naperville:InitialY"] ?? MapConstants.DefaultY.ToString());
                            var scale = int.Parse(configuration["MapPopup:Naperville:InitialScale"] ?? MapConstants.DefaultScale.ToString());

                            MapPoint mapPoint = new(x, y, SpatialReferences.Wgs84);
                            Viewpoint viewpoint = new(mapPoint, scale);

                            MapConfig config = new()
                            {
                                LoadLayers = true,
                                LocationName = LocationName.Naperville,
                                MmpkPath = UtilityService.GetFilePathFromDataFolder("Naperville", "NapervilleGas.mmpk"),
                                VtpkPath = UtilityService.GetFilePathFromDataFolder("Naperville", "Naperville.vtpk"),
                                Viewpoint = viewpoint
                            };
                            return config;
                        }
                    default:
                        break;
                }
            }
            return new();
        }
    }
}
