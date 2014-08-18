using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;


namespace Mle.Xaml.Converters {
    public class BooleanStyleConverter : IValueConverter {
        SolidColorBrush defaultBrush = Application.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush;
        SolidColorBrush accentBrush = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool original = (bool)value;
            if (original)
                return accentBrush;
            else
                return defaultBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}