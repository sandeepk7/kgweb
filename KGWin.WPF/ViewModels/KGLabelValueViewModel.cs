using KGWin.WPF.ViewModels.Base;

namespace KGWin.WPF.ViewModels
{
    public class KGLabelValueViewModel : ViewModelBase
    {
        private string _label = "";
        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        private string _value = "";
        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }
}
