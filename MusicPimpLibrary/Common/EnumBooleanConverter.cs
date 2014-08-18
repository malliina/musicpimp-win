using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;


namespace Mle.Common {
    public class EnumBooleanConverter : IValueConverter {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, string language) {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString, ignoreCase: true);
            return parameterValue.Equals(value);
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, string language) {
            string parameterString = parameter as string;
            if (parameterString == null || value.Equals(false)) {
                return DependencyProperty.UnsetValue;
            }
            return Enum.Parse(targetType, parameterString, ignoreCase: true);
        }
        #endregion
    }
}