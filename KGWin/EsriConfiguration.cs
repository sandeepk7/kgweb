using Microsoft.Extensions.Configuration;
using System.IO;

namespace KGWin
{
    public static class EsriConfiguration
    {
        private static IConfiguration? _configuration;
        
        public static IConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    
                    _configuration = builder.Build();
                }
                return _configuration;
            }
        }
        
        public static string ApiKey => Configuration["EsriSettings:ApiKey"] ?? string.Empty;
        public static string PortalUrl => Configuration["EsriSettings:PortalUrl"] ?? "https://www.arcgis.com";
    }
}
