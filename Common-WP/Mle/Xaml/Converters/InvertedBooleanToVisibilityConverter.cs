using System;
using System.Globalization;


namespace Mle.Xaml.Converters {
    public class InvertedBooleanToVisibilityConverter : BooleanToVisibilityConverter {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var boolValue = (bool)value;
            return base.Convert(!boolValue, targetType, parameter, culture);
        }
    }
}