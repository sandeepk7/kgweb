using KGWin.WPF.ViewModels.Base;
using Microsoft.Extensions.Configuration;

namespace KGWin.WPF.ViewModels
{
    public class ModalPopupWindowViewModel : ViewModelBase
    {
        public ModalPopupWindowViewModel()
        {
            _title = "Modal Popup Window";
        }

        private object? _popupContent;
        private string _title;

        public object? PopupContent
        {
            get => _popupContent;
            set => SetProperty(ref _popupContent, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public async Task Initialize()
        {
        }
    }
}
