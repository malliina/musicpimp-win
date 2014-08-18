using Callisto.Controls;
using Mle.MusicPimp.ViewModels;
using Mle.MusicPimp.Xaml.Controls;
using Mle.Xaml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mle.MusicPimp.Xaml {
    public sealed partial class Settings : PopupUserControl {

        public SettingsViewModel ViewModel { get; private set; }

        public Settings() {
            ViewModel = SettingsViewModel.Instance;
            DataContext = ViewModel;
            this.InitializeComponent();
            // TODO jfc fix
            var t = ViewModel.Limits.CalculateConsumedGb();
        }

        private void AddEndpointClicked(object sender, RoutedEventArgs e) {
            CloseThisPopup();
            PopupManager.Show(new AddEndpointPopup());
        }

        private void OnEndpointSelected(object sender, SelectionChangedEventArgs e) {
            var endpoint = (MusicEndpoint)EndpointsListBox.SelectedItem;
            if (endpoint != null && endpoint.EndpointType != EndpointTypes.Local) {
                CloseThisPopup();
                var flyoutContent = new EndpointPanel(endpoint.Name);
                var flyout = FlyoutManager.NewDefaultFlyout("Edit endpoint", flyoutContent);
                flyoutContent.ViewModel.CloseRequested += () => returnToSettings(flyout);
                flyout.BackClicked += (s, be) => {
                    be.Cancel = true;
                    returnToSettings(flyout);
                };
                flyout.IsOpen = true;
            }
        }
        private void returnToSettings(SettingsFlyout from) {
            from.IsOpen = false;
            FlyoutManager.OpenFlyout<Settings>("Options");
        }
    }
}
