using System.Windows;

namespace KGWin
{
    /// <summary>
    /// Interaction logic for AssetPopup.xaml
    /// </summary>
    public partial class AssetPopup : Window
    {
        public AssetPopup()
        {
            InitializeComponent();
        }

        public void SetAssetInformation(string assetId, string assetName, string assetType, string description)
        {
            AssetIdText.Text = assetId;
            AssetNameText.Text = assetName;
            AssetTypeText.Text = assetType;
            DescriptionText.Text = description;
        }

        public void SetPosition(double x, double y)
        {
            this.Left = x;
            this.Top = y;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
