using KGWin.WPF.Interfaces;
using KGWin.WPF.Models;
using KGWin.WPF.ViewModels;
using KGWin.WPF.Views;
using KGWin.WPF.Views.Map;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace KGWin.WPF.Services
{
    public class WebRequestProcessor : IWebRequestProcessor
    {
        public WebRequestProcessor(IConfiguration configuration, IAuthService authService)
        {
            _configuration = configuration;
            _authService = authService;
        }

        private IConfiguration _configuration;
        private IAuthService _authService;

        public async Task ProcessRequestAsync(string messageJson)
        {
            if (messageJson != null)
            {
                using var jsonDoc = JsonDocument.Parse(messageJson);
                string typeName = jsonDoc.RootElement.GetProperty("type").ToString();

                FromJsRequestType type = Enum.Parse<FromJsRequestType>(typeName);

                switch (type)
                {
                    case FromJsRequestType.NapervillePopupWindow:
                        await ProcessNapervillePopupWindowRequestAsync();
                        break;
                    default:
                        break;
                }
            }
        }

        private async Task ProcessNapervillePopupWindowRequestAsync()
        {
            var isAuthenticated = await _authService.CheckUserAuthenticated();
            if (!isAuthenticated) return;

            var config = MapConfig.GetConfig(LocationName.Naperville, _configuration);

            KGMap napervilleMap = new();
            var mapViewModel = (KGMapViewModel)napervilleMap.DataContext;
            await mapViewModel.InitializeAsync(config);

            KGModalPopupWindow popup = new();
            var popupViewModel = (KGModalPopupWindowViewModel)popup.DataContext;
            popupViewModel.PopupContent = napervilleMap;
            popup.Show();
        }
    }
}
