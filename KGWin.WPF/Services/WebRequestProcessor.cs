using KGWin.WPF.Interfaces;
using KGWin.WPF.Models;
using KGWin.WPF.ViewModels;
using KGWin.WPF.Views;
using KGWin.WPF.Views.Map;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

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
                    case FromJsRequestType.UserInfo:
                        ProcessUserInfo(messageJson);
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

        private void ProcessUserInfo( string messageJson)
        {
            var webUserName = UtilityService.DeserializeJson<KGWebMessage<string>>(messageJson);

            _authService.WebUser = webUserName!.Data;
            var result = _authService.CheckWebUserLicensed();

            if (result == true)
            {
                MessageBox.Show($"Welcome, {_authService.WebUser}!", "Login Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            if (result == false)
            {
                MessageBox.Show($"{_authService.WebUser}!", "Login Again", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
