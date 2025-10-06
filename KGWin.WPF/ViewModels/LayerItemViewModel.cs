using Esri.ArcGISRuntime.Mapping;
using KGWin.WPF.ViewModels.Base;

namespace KGWin.WPF.ViewModels
{
    public class LayerItemViewModel : ViewModelBase
    {
        private bool _isVisible = true;
        private string _name = "";
        private string _layerType = "";
        private string _description = "";
        private string _fullExtent = "";
        private string _minScale = "";
        private string _maxScale = "";
        private string _itemId = "";
        private string _url = "";
        private string _popupTitle = "";
        private string _popupFields = "";
        private bool _hasPopupDefinition = false;

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string LayerType
        {
            get => _layerType;
            set => SetProperty(ref _layerType, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string FullExtent
        {
            get => _fullExtent;
            set => SetProperty(ref _fullExtent, value);
        }

        public string MinScale
        {
            get => _minScale;
            set => SetProperty(ref _minScale, value);
        }

        public string MaxScale
        {
            get => _maxScale;
            set => SetProperty(ref _maxScale, value);
        }

        public string ItemId
        {
            get => _itemId;
            set => SetProperty(ref _itemId, value);
        }

        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        public string PopupTitle
        {
            get => _popupTitle;
            set => SetProperty(ref _popupTitle, value);
        }

        public string PopupFields
        {
            get => _popupFields;
            set => SetProperty(ref _popupFields, value);
        }

        public bool HasPopupDefinition
        {
            get => _hasPopupDefinition;
            set => SetProperty(ref _hasPopupDefinition, value);
        }

        public Layer? Layer { get; set; }
    }
}
