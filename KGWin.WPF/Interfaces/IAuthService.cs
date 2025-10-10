﻿namespace KGWin.WPF.Interfaces
{
    public interface IAuthService
    {
        bool IsUserAuthenticated { get; }
        string? AuthenticatedUserName { get; }
        Task LoginAsync();
        Task<bool> CheckUserAuthenticated();
        string? WebUser { get; set; }
        bool CheckWebUserLicensed();
    }
}