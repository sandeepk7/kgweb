using System.Windows;
using System.Windows.Controls;

namespace KGWin.WPF.Views.Components
{
    /// <summary>
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class KGPopup : UserControl
    {
        public static readonly DependencyProperty BodyProperty = DependencyProperty.Register(
            nameof(Body),
            typeof(object),
            typeof(KGPopup),
            new PropertyMetadata(null));

        public object? Body
        {
            get => GetValue(BodyProperty);
            set => SetValue(BodyProperty, value);
        }

        public KGPopup()
        {
            InitializeComponent();
        }
    }
}
