using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Mle.Xaml {
    public class Flyouts {
        //public static Callisto.Controls.SettingsFlyout NewDefaultFlyout(string header, UserControl content) {
        //    var flyout = new Callisto.Controls.SettingsFlyout();
        //    flyout.HeaderBrush = Application.Current.Resources["AppColor"] as SolidColorBrush;
        //    flyout.HeaderText = header;
        //    var bmp = new BitmapImage(new Uri("ms-appx:///Assets/SmallLogo.png"));
        //    flyout.SmallLogoImageSource = bmp;
        //    flyout.Content = content;
        //    return flyout;
        //}
        public static SettingsFlyout Build(string header, UserControl content) {
            return new SettingsFlyout() {
                HeaderBackground = Application.Current.Resources["AppColor"] as SolidColorBrush,
                Title = header,
                IconSource = new BitmapImage(new Uri("ms-appx:///Assets/SmallLogo.png")),
                Content = content
            };
        }
    }
}
