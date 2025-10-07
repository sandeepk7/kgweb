namespace KGWin.WPF.Interfaces
{
    public interface ICommunicationService : ICefSharpCommunicationService, IWebViewCommunicationService
    {
        Task RaiseCreateWorkOrderAsync(string workOrderData);
    }
}
