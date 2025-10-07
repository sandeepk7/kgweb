using KGWin.WPF.ViewModels.Base;
using System.Windows.Input;

namespace KGWin.WPF.ViewModels
{
    public class KGMapControlToolbarViewModel : ViewModelBase
    {
        public ICommand LassoClickedCommand { get; set; }
    }
}
