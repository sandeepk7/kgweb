using System.Diagnostics;
using System.Text.Json;
using System.Windows.Controls;

namespace KGWin.WPF.Views.Map
{
    /// <summary>
    /// Interaction logic for MapLayerManager.xaml
    /// </summary>
    public partial class KGMapLayerManager : UserControl
    {
        public KGMapLayerManager()
        {
            InitializeComponent();

            Debug.WriteLine(JsonSerializer.Serialize(DataContext));
        }
    }
}
