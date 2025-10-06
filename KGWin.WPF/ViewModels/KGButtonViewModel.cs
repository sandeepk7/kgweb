using KGWin.WPF.ViewModels.Base;
using System.Windows.Input;

namespace KGWin.WPF.ViewModels
{
    public class KGButtonViewModel : ViewModelBase
    {
        public string _buttonContent = string.Empty;
        public string ButtonContent
        {
            get => _buttonContent;
            set => SetProperty(ref _buttonContent, value);
        }

        public ICommand ButtonCommand { get; set; } = null!;
    }
}
