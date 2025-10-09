using KGWin.WPF.Interfaces;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Security;
using Microsoft.Extensions.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using CredentialManagement;
using Credential = CredentialManagement.Credential;
using KGWin.WPF.Views;
using Windows.System;

namespace KGWin.WPF.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly IConfiguration _configuration;

        public LicenseService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> SaveLicense()
        {
            string loginUrl = _configuration["ArcGISLogin:ArcGISUrl"]!;
            string Key  = _configuration["ArcGISLogin:ArcGISLicenseKey"]!; 

            var portal = await ArcGISPortal.CreateAsync(new Uri(loginUrl), loginRequired: true);

            var user = portal.User;

            Key = Key+ "-" + user!.UserName;

            PortalQueryParameters queryParams = new PortalQueryParameters("owner:" + user!.UserName);

            // Execute query
            PortalQueryResultSet<PortalItem> resultSet = await portal.FindItemsAsync(queryParams);

            // Optional: Apply license
            var licenseInfo = await portal.GetLicenseInfoAsync();

            var licenseResult = ArcGISRuntimeEnvironment.SetLicense(licenseInfo);

            //SaveCredentials(user.UserName, licenseInfo.ToJson(), Key);

            bool isValid = IsLicenseValid(user.UserName);
            return user!.UserName;
        }


        public void SaveCredentials(string username, string license, string key)
        {
            var credential = new Credential
            {
                Username = username,
                Password = license,
                Target = key,
                PersistanceType = PersistanceType.LocalComputer,
                Type = CredentialType.Generic
            };

            credential.Save();
        }

        public bool IsLicenseValid(string username)
        {
            string key = _configuration["ArcGISLogin:ArcGISLicenseKey"]! + "-" + username;
            bool result = false;
            var cred = new Credential { Target = key };
            if (cred.Load() &&
                string.Equals(cred.Username ?? string.Empty, username, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(cred.Password))
            {
                DateTime dateTime = cred.LastWriteTimeUtc;
                if(DateTime.UtcNow.Date <=  dateTime.Date.AddDays(30))
                {
                    LicenseInfo licenseInfo = LicenseInfo.FromJson(cred.Password!)!;
                    var licenseResult = ArcGISRuntimeEnvironment.SetLicense(licenseInfo!);
                    result = true;
                }                
            }

            return result;
        }
    }
}