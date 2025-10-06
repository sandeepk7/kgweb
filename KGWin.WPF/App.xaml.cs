using KGWin.WPF.Interfaces;
using KGWin.WPF.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace KGWin.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Services = RegsiterService.Init();
            Initialize();
        }

        private void Initialize()
        {
            var arcGisSerivce = Services.GetRequiredService<IArcGisService>();
            arcGisSerivce.Initialize();
        }
    }
}
