using KGWin.WPF.ViewModels;
using KGWin.WPF.ViewModels.Base;
using System.Windows;
using System.Windows.Controls;

namespace KGWin.WPF.Views.Map
{
    /// <summary>
    /// Interaction logic for KGMapToolbar.xaml
    /// </summary>
    public partial class KGMapControlToolbar : UserControl
    {
        public const string LassoButtonClick = $"{nameof(LassoButton)}{nameof(LassoButton.Click)}";

        public KGMapControlToolbar()
        {
            InitializeComponent();
            this.DataContextChanged += KGMapControlToolbar_DataContextChanged;
        }

        private void KGMapControlToolbar_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as KGMapControlToolbarViewModel;
            if (vm != null)
            {
                var eventAggregator = vm.EventAggregator;

                LassoButton.Click += (sender, eventArg) => eventAggregator.Publish(new KGEvent(LassoButtonClick, sender, eventArg));
            }
        }
    }
}
