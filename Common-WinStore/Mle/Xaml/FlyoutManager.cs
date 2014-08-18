using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Mle.Xaml {
    public class FlyoutManager : PopupManager {

        public static void OpenFlyout<T>(string header) where T : UserControl, new() {
            OpenFlyout(header, new T());
        }

        public static void OpenFlyout(string header, UserControl content) {
            var flyout = NewDefaultFlyout(header, content);
            flyout.IsOpen = true;
        }
        //public static void OpenFlyout(string header, Func<UserControl> content, Action onBack) {
        //    var flyout = NewDefaultFlyout(header, content);
        //    flyout.BackClicked += (s, be) => onBack();
        //    flyout.IsOpen = true;
        //}
        public static Callisto.Controls.SettingsFlyout NewDefaultFlyout(string header, UserControl content) {
            var flyout = new Callisto.Controls.SettingsFlyout();
            flyout.HeaderBrush = Application.Current.Resources["AppColor"] as SolidColorBrush;
            flyout.HeaderText = header;
            var bmp = new BitmapImage(new Uri("ms-appx:///Assets/SmallLogo.png"));
            flyout.SmallLogoImageSource = bmp;
            flyout.Content = content;
            return flyout;
        }

        private const int width = 346;
        protected FlyoutManager(UserControl flyoutContent)
            : base(flyoutContent) {
            Window.Current.SizeChanged += OnWindowSizeChanged;
        }

        protected override void BeforeOpen() {
            popup.IsLightDismissEnabled = true;
            popup.Width = width;
            popup.Height = bounds.Height;
            popupContent.Width = width;
            popupContent.Height = bounds.Height;
            popup.SetValue(Canvas.LeftProperty, bounds.Width - width);
            popup.SetValue(Canvas.TopProperty, 0);
        }

        void OnWindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e) {
            bounds = Window.Current.Bounds;
        }
    }
}
