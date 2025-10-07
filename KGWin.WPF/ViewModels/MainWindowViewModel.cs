using Esri.ArcGISRuntime.Security;
using KGWin.WPF.Interfaces;
using KGWin.WPF.Services;
using KGWin.WPF.ViewModels.Base;
using System.Windows;

namespace KGWin.WPF.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(              
              ILoginService loginService,
              KGButtonViewModel kGButtonViewModel
          )
        {
            _loginService = loginService;

           
            kGButtonViewModel.ButtonContent = "Login";
            kGButtonViewModel.ButtonCommand = new RelayCommand(Login_btn);
           _kGButtonViewModel = kGButtonViewModel;
        }

        private ILoginService _loginService;

        private KGButtonViewModel _kGButtonViewModel;
        public KGButtonViewModel KGButtonViewModel { 
            get => _kGButtonViewModel;

            set => SetProperty(ref _kGButtonViewModel, value);
        
        }
       
        public async void Login_btn()
        {

            LoginService.SetChallengeHandler();

            //bool loggedIn = await ArcGISLoginPrompt.EnsureAGOLCredentialAsync();

            try
            {
                var requestInfo = new CredentialRequestInfo
                {
                    ServiceUri = new Uri("https://www.arcgis.com"),
                    AuthenticationType = AuthenticationType.Token
                };

                var credential = await AuthenticationManager.Current.GetCredentialAsync(requestInfo, false);


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
