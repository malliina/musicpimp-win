using Mle.MusicPimp.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Mle.MusicPimp.Xaml.Converters {
    /// <summary>
    /// Visible for non-local endpoints, collapsed otherwise.
    /// </summary>
    public class EndpointTypeToVisibilityConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, string language) {
            EndpointTypes original = (EndpointTypes)value;
            if (original == EndpointTypes.Local){
                return Visibility.Collapsed;
            }else{
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
