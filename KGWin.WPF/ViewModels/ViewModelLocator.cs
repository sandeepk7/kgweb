using Microsoft.Extensions.DependencyInjection;

namespace KGWin.WPF.ViewModels
{
    public class ViewModelLocator
    {
        public KGWebViewModel KGWebViewModel => App.Services.GetRequiredService<KGWebViewModel>();
        public KGMapViewModel MapViewModel => App.Services.GetRequiredService<KGMapViewModel>();
        public LayerItemViewModel LayerItemViewModel => App.Services.GetRequiredService<LayerItemViewModel>();
        public ModalPopupWindowViewModel ModalPopupViewModel => App.Services.GetRequiredService<ModalPopupWindowViewModel>();
        public KGPopupViewModel PopupViewModel => App.Services.GetRequiredService<KGPopupViewModel>();
        public KGButtonViewModel KGButtonViewModel => App.Services.GetRequiredService<KGButtonViewModel>();
        
    }
}
