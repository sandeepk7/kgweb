using KGWin.WPF.Interfaces;
using KGWin.WPF.Models;
using System.Text.Json;
using Microsoft.Web.WebView2.Wpf;

namespace KGWin.WPF.Services
{
    public class CommunicationService : ICommunicationService
    {
        public CommunicationService(IWebRequestProcessor requestProcessor)
        {
            _requestProcessor = requestProcessor;
        }

        private IWebRequestProcessor _requestProcessor;
        private WebView2? _kgWebView;

        //public void Initialize(CefSharp browser)
        //{
        //}

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

        public async Task RaiseCreateWorkOrder(string workOrderData)
        {
            if (_kgWebView != null) {                
                await _kgWebView.CoreWebView2.ExecuteScriptAsync($"requestFromWpf({JSRequestType.CreateWorkOrder},'{workOrderData}')");
            }
        }
    }
}
