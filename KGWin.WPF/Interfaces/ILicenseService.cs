using Esri.ArcGISRuntime.Portal;

namespace KGWin.WPF.Interfaces
{
    public interface ILicenseService
    {
        public Task<string> SaveLicense();

        public void SaveCredentials(string username, string license, string key);

        public bool IsLicenseValid(string username);
    }
}