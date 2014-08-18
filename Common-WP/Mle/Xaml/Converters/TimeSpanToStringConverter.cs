using Mle.Util;
using System;
using System.Globalization;
using System.Windows.Data;


namespace Mle.Xaml.Converters {
    public class TimeSpanToStringConverter : IValueConverter {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            TimeSpan original = (TimeSpan)value;
            return original.ToMyFormat();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}