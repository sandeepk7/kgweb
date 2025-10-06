using KGWin.WPF.Models;

namespace KGWin.WPF.Interfaces
{
    public interface IWebRequestProcessor
    {
        Task ProcessRequestAsync(string messageJson);
    }
}
