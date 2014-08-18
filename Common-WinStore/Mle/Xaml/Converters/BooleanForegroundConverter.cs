using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Mle.Xaml.Converters {
    public class BooleanForegroundConverter : IValueConverter {
        SolidColorBrush defaultBrush = Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush;
        SolidColorBrush accentBrush = Application.Current.Resources["HyperlinkForegroundThemeBrush"] as SolidColorBrush;

        public object Convert(object value, Type targetType, object parameter, string language) {
            bool original = (bool)value;
            if (original)
                return accentBrush;
            else
                return defaultBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
