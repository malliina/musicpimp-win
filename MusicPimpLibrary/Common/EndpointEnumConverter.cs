using Mle.MusicPimp.ViewModels;
using System;
using Windows.UI.Xaml;

namespace Mle.Common {
    public class EndpointEnumConverter : GenericEnumConverter<EndpointTypes> { }
    public class ProtocolEnumConverter : GenericEnumConverter<Protocols> { }
    public class StartPageEnumConverter : GenericEnumConverter<Pages> { }
    public class GenericEnumConverter<T> : EnumBooleanConverter {
        public override object ConvertBack(object value, Type targetType, object parameter, string language) {
            string parameterString = parameter as string;
            if (parameterString == null || value.Equals(false)) {
                return DependencyProperty.UnsetValue;
            }
            return Enum.Parse(typeof(T), parameterString, ignoreCase: true);
        }
    }
}
