using KGWin.WPF.Interfaces;
using Microsoft.Extensions.Configuration;

namespace KGWin.WPF.Services
{
    public partial class CommunicationService : ICommunicationService
    {
        public CommunicationService(
            IConfiguration configuration,
            IWebRequestProcessor requestProcessor)
        {
            _configuration = configuration;
            _requestProcessor = requestProcessor;

            if (bool.TryParse(_configuration["Web:UseCefSharp"], out bool useCefSharp))
            {
                _useCefSharp = useCefSharp;
            }
        }

        private IWebRequestProcessor _requestProcessor;
        private IConfiguration _configuration;
        private bool _useCefSharp;

        public async Task RaiseCreateWorkOrderAsync(string workOrderData)
        {
            if (_useCefSharp)
            {
            }
            else
            {
                await RaiseCreateWorkOrderWebViewAsync(workOrderData);
            }
        }
    }
}
