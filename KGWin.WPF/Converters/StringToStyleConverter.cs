using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KGWin.WPF.Converters
{
    //public class StringToStyleConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value is string currentPage && parameter is string targetPage)
    //        {
    //            // Select style key and resolve actual Style from resources
    //            var styleKey = currentPage == targetPage ? "ActiveNavigationButtonStyle" : "NavigationButtonStyle";
    //            // Prefer Window resources so we can find styles defined in MainWindow.xaml
    //            var resolved = Application.Current.MainWindow?.TryFindResource(styleKey)
    //                          ?? Application.Current.TryFindResource(styleKey);
    //            return resolved ?? DependencyProperty.UnsetValue;
    //        }
    //        var fallback = Application.Current.MainWindow?.TryFindResource("NavigationButtonStyle")
    //                       ?? Application.Current.TryFindResource("NavigationButtonStyle");
    //        return fallback ?? DependencyProperty.UnsetValue;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
