using System;
using Windows.UI.Xaml.Data;

namespace Mle.Xaml.Converters {
    public class TimeSpanToDouble : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            TimeSpan original = (TimeSpan)value;
            return original.TotalSeconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            double original = (double)value;
            return TimeSpan.FromSeconds(original);
        }
    }
}