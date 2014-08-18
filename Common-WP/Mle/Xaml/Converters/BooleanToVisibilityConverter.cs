using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


namespace Mle.Xaml.Converters {
    public class BooleanToVisibilityConverter : IValueConverter {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool original = (bool)value;
            if (original)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}