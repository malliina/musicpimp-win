using System;
using System.Globalization;
using System.Windows.Data;

namespace Mle.Xaml.Converters {
    public class TimeSpanToDouble : IValueConverter {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            TimeSpan original = (TimeSpan)value;
            return original.TotalSeconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            double original = (double)value;
            return TimeSpan.FromSeconds(original);
        }
    }
}