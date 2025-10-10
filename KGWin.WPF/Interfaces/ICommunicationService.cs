namespace KGWin.WPF.Interfaces
{
    public interface ICommunicationService : IWebViewCommunicationService
    {
        Task RaiseCreateWorkOrderAsync(string workOrderData);
    }
}
