using Mle.Util;
using System;
using Windows.UI.Xaml.Data;


namespace Mle.Xaml.Converters {
    public class TimeSpanToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            TimeSpan original = (TimeSpan)value;
            return original.ToMyFormat();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}