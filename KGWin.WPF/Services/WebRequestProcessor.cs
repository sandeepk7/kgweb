using KGWin.WPF.Interfaces;
using KGWin.WPF.Models;
using KGWin.WPF.ViewModels;
using KGWin.WPF.Views.Components;
using KGWin.WPF.Views.Map;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Windows;

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
            var config = MapConfig.GetConfig(LocationName.Naperville, _configuration);

            KGMap napervilleMap = new();
            var mapViewModel = (KGMapViewModel)napervilleMap.DataContext;
            await mapViewModel.InitializeAsync(config);

            KGModalPopupWindow popup = new()
            {
                Owner = Application.Current.MainWindow,
            };
            var popupViewModel = (KGModalPopupWindowViewModel)popup.DataContext;
            popupViewModel.PopupContent = napervilleMap;
            popup.Show();


            var configuredTitle = _configuration["MapPopup:Naperville:ModalTitle"];
            if (!string.IsNullOrWhiteSpace(configuredTitle))
            {
                popupViewModel.Title = configuredTitle;
            }

            var widthStr = _configuration["MapPopup:Naperville:Width"];
            var heightStr = _configuration["MapPopup:Naperville:Height"];
            var fitContentStr = _configuration["MapPopup:Naperville:FitContent"];

            // Parse manually
            double? width = double.TryParse(widthStr, out var w) ? w : (double?)null;
            double? height = double.TryParse(heightStr, out var h) ? h : (double?)null;
            bool fitContent = bool.TryParse(fitContentStr, out var f) ? f : false;

            if (fitContent)
            {
                popup.SizeToContent = SizeToContent.WidthAndHeight;
            }
            else
            {
                popup.SizeToContent = SizeToContent.Manual;
                if (width.HasValue && width.Value > 0) popup.Width = width.Value;
                if (height.HasValue && height.Value > 0) popup.Height = height.Value;
            }
            popup.ResizeMode = System.Windows.ResizeMode.CanResize;
        }
    }
}
