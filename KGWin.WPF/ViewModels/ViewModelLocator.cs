using Microsoft.Extensions.DependencyInjection;

namespace KGWin.WPF.ViewModels
{
    public class ViewModelLocator
    {
        public KGWebViewModel KGWebViewModel => App.Services.GetRequiredService<KGWebViewModel>();
        public KGMapViewModel MapViewModel => App.Services.GetRequiredService<KGMapViewModel>();
        public KGLayerItemViewModel LayerItemViewModel => App.Services.GetRequiredService<KGLayerItemViewModel>();
        public KGModalPopupWindowViewModel ModalPopupViewModel => App.Services.GetRequiredService<KGModalPopupWindowViewModel>();
        public KGPopupViewModel PopupViewModel => App.Services.GetRequiredService<KGPopupViewModel>();
        public KGButtonViewModel KGButtonViewModel => App.Services.GetRequiredService<KGButtonViewModel>();
        public KGMapControlToolbarViewModel KGMapControlToolbarViewModel => App.Services.GetRequiredService<KGMapControlToolbarViewModel>();
    }
}
