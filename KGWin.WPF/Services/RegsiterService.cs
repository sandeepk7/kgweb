using KGWin.WPF.Interfaces;
using KGWin.WPF.ViewModels;
using KGWin.WPF.ViewModels.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace KGWin.WPF.Services
{
    public class RegsiterService
    {
        public static IServiceProvider Init()
        {
            var services = new ServiceCollection();
            RegisterConfiguration(services);
            RegisterServices(services);
            RegisterViewModels(services);

            return services.BuildServiceProvider();
        }

        private static void RegisterConfiguration(IServiceCollection services)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                           ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                           ?? (Debugger.IsAttached ? "Development" : "Production");

            Debug.WriteLine($"AppSettingService: Loading environment: {environment}");
            Debug.WriteLine($"AppSettingService: Debugger attached: {Debugger.IsAttached}");

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: environment == "Production", reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            services.AddSingleton<IConfiguration>(configuration);
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ICommunicationService, CommunicationService>();
            services.AddTransient<IArcGisService, ArcGisService>();
            services.AddTransient<IWebRequestProcessor, WebRequestProcessor>();
            services.AddTransient<EventAggregator>();
            services.AddTransient<IMapDrawService, MapDrawService>();
            


        }

        private static void RegisterViewModels(IServiceCollection services)
        {
            services.AddTransient<KGWebViewModel>();
            services.AddTransient<KGMapViewModel>();
            services.AddTransient<KGLayerItemViewModel>();
            services.AddTransient<KGModalPopupWindowViewModel>();
            services.AddTransient<KGPopupViewModel>();
            services.AddTransient<KGButtonViewModel>();
            services.AddTransient<KGLabelValueViewModel>();
            services.AddTransient<KGMapControlToolbarViewModel>();            
        }    
    }
}
