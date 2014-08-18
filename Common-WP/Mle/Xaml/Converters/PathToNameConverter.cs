using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;


namespace Mle.Xaml.Converters {
    public class PathToNameConverter : IValueConverter {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Path.GetFileName((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}