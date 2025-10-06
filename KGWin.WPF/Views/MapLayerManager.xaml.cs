using System.Diagnostics;
using System.Text.Json;
using System.Windows.Controls;

namespace KGWin.WPF.Views
{
    /// <summary>
    /// Interaction logic for MapLayerManager.xaml
    /// </summary>
    public partial class MapLayerManager : UserControl
    {
        public MapLayerManager()
        {
            InitializeComponent();

            Debug.WriteLine(JsonSerializer.Serialize(DataContext));
        }
    }
}
