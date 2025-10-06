using KGWin.WPF.Models;
using Microsoft.Web.WebView2.Wpf;

namespace KGWin.WPF.Interfaces
{
    public interface ICommunicationService
    {
        void Initialize(WebView2 kgWebView);
    }
}
