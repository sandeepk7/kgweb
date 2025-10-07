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
        public WebRequestProcessor(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IConfiguration _configuration { get; set; }

        public async Task ProcessRequestAsync(string messageJson)
        {
            if (messageJson != null)
            {
                using var jsonDoc = JsonDocument.Parse(messageJson);
                string typeName = jsonDoc.RootElement.GetProperty("type").ToString();

                RequestType type = Enum.Parse<RequestType>(typeName);

                switch (type)
                {
                    case RequestType.NapervillePopupWindow:
                        await ProcessNapervillePopupWindowRequestAsync();
                        break;
                    default:
                        break;
                }
            }
        }

        private async Task ProcessNapervillePopupWindowRequestAsync()
        {
            var config = MapConfig.GetConfig(LocationName.Naperville, _configuration);

            KGMap napervilleMap = new();
            var mapViewModel = (KGMapViewModel)napervilleMap.DataContext;
            await mapViewModel.InitializeAsync(config);

            KGModalPopupWindow popup = new();
            var popupViewModel = (ModalPopupWindowViewModel)popup.DataContext;
            popupViewModel.PopupContent = napervilleMap;
            popup.Show();
        }
    }
}
