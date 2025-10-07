using CefSharp.Wpf;
using KGWin.WPF.Interfaces;

namespace KGWin.WPF.Services
{
    public partial class CommunicationService : ICefSharpCommunicationService
    {
        private ChromiumWebBrowser? _browser;

        public void Initialize(ChromiumWebBrowser browser)
        {
            _browser = browser;
        }
    }
}
