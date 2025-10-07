using CefSharp.Wpf;

namespace KGWin.WPF.Interfaces
{
    public interface ICefSharpCommunicationService
    {
        void Initialize(ChromiumWebBrowser browser);
    }
}
