using KGWin.WPF.Interfaces;
using KGWin.WPF.ViewModels.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.IO;

namespace KGWin.WPF.ViewModels
{
    public class KGWebViewModel : ViewModelBase
    {
        public KGWebViewModel(IConfiguration configuration, ICommunicationService communicationService)
        {
            _configuration = configuration;
            _communicationService = communicationService;
            _url = _configuration["Web:Url"]!;

            var cachePath = _configuration["WebView:CachePath"]!;
            _userDataFolder = Path.Combine(_localAppData, cachePath);
            Directory.CreateDirectory(_userDataFolder);
        }

        private static readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private IConfiguration _configuration;
        private ICommunicationService _communicationService;
        private string _url;
        private string _userDataFolder;
        private WebView2 _kgWebView;

        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        public string UserDataFolder
        {
            get => _userDataFolder;
            set {
                bool isSet = SetProperty(ref _userDataFolder, value);

                if(isSet && !string.IsNullOrWhiteSpace(value) && _kgWebView != null)
                {
                    _kgWebView.CoreWebView2.Navigate(value);
                }
            }
        }

        public async Task InitializeAsync(WebView2 kgWebView)
        {
            _kgWebView = kgWebView;

            var env = await CoreWebView2Environment.CreateAsync(userDataFolder: UserDataFolder);
            await _kgWebView.EnsureCoreWebView2Async(env);

            // Navigate to initial URL if set
            if (!string.IsNullOrEmpty(Url))
            {
                _kgWebView.CoreWebView2.Navigate(Url);
            }

            _communicationService.Initialize(_kgWebView);
        }
    }
}
