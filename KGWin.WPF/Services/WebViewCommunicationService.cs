using KGWin.WPF.Interfaces;
using KGWin.WPF.Models;
using Microsoft.Web.WebView2.Wpf;

namespace KGWin.WPF.Services
{
    public partial class CommunicationService : IWebViewCommunicationService
    {
        private WebView2? _kgWebView;

        public void Initialize(WebView2 kgWebView)
        {
            _kgWebView = kgWebView;

            _kgWebView.CoreWebView2.WebMessageReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.WebMessageAsJson))
                {
                    _requestProcessor.ProcessRequestAsync(args.WebMessageAsJson);
                }
            };
        }

        public async Task RaiseCreateWorkOrderWebViewAsync(string workOrderData)
        {
            if (_kgWebView != null)
            {
                await _kgWebView.CoreWebView2.ExecuteScriptAsync($"requestFromWpf({ToJsRequestType.CreateWorkOrder},'{workOrderData}')");
            }
        }
    }
}
