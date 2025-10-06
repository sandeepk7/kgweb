using Esri.ArcGISRuntime.Mapping;
using KGWin.WPF.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace KGWin.WPF.ViewModels
{
    public class KGPopupViewModel : ViewModelBase
    {
        public KGPopupViewModel()
        {
            _isVisible = false;
            _width = 450;
            _height = 480;
            InitializeCommands();
        }

        private string _title = string.Empty;
        private bool _isVisible;
        private double _x;
        private double _y;
        private int _width;
        private int _height;

        private ObservableCollection<KGButtonViewModel> _buttons = [];

       // public static readonly DependencyProperty ContentProperty =
       //DependencyProperty.Register(nameof(Body), typeof(object), typeof(MyUserControl));

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }
        public double X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }
        public double Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }
        public int Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }
        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public ObservableCollection<KGButtonViewModel> Buttons
        {
            get => _buttons;
            set => SetProperty(ref _buttons, value);
        }
        public ICommand ClosePopupCommand { get; private set; } = null!;

        private void InitializeCommands()
        {
            ClosePopupCommand = new RelayCommand(ClosePopup);
        }

        public void ClosePopup()
        {
            IsVisible = false;
            Title = "";
        }
    }
}
