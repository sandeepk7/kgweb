using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Security;
using KGWin.WPF.Interfaces;
using KGWin.WPF.Services;
using KGWin.WPF.ViewModels.Base;
using Microsoft.Extensions.Configuration;
using System.Windows;

namespace KGWin.WPF.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(IConfiguration configuration,ILoginService loginService,KGButtonViewModel kGButtonViewModel
          )
        {
            _loginService = loginService;
            _configuration = configuration;

            kGButtonViewModel.ButtonContent = "Login";
            kGButtonViewModel.ButtonCommand = new RelayCommand(Login_btn);
           _kGButtonViewModel = kGButtonViewModel;
        }

        private ILoginService _loginService;
        IConfiguration _configuration;

        private KGButtonViewModel _kGButtonViewModel;
        public KGButtonViewModel KGButtonViewModel { 
            get => _kGButtonViewModel;

            set => SetProperty(ref _kGButtonViewModel, value);
        
        }
       
        public async void Login_btn()
        {

            string LoginUrl = _configuration["ArcGISLogin:ArcGISUrl"]!;
            LoginService.SetChallengeHandler(_configuration);

            try
            {
                var requestInfo = new CredentialRequestInfo
                {
                    ServiceUri = new Uri(LoginUrl),
                    AuthenticationType = AuthenticationType.Token
                };

                var credential = await AuthenticationManager.Current.GetCredentialAsync(requestInfo, false);


                var portal = await ArcGISPortal.CreateAsync(new Uri(LoginUrl), loginRequired: true);

                var user = portal.User;


                PortalQueryParameters queryParams = new PortalQueryParameters("owner:" + user.UserName);

                // Execute query
                PortalQueryResultSet<PortalItem> resultSet = await portal.FindItemsAsync(queryParams);


                var licenseInfo = await portal.GetLicenseInfoAsync();
                
                var licenseResult = ArcGISRuntimeEnvironment.SetLicense(licenseInfo);


              


                if (credential != null)
                {
                    // StatusText.Text = "Authenticated!";
                    MessageBox.Show("Authenticated Successfull");

                }
                else
                {
                 //   StatusText.Text = "Authentication canceled or failed.";
                }
            }
            catch (Exception ex)
            {
                //StatusText.Text = "Login failed: " + ex.Message;
                MessageBox.Show("Login failed: " + ex.Message);
            }   
        }
    }
}
