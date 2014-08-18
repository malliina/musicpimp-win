using Microsoft.Phone.Controls;
using Mle.MusicPimp.Phone.Controls;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using Mle.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Pages {
    public partial class Settings : AsyncPhoneApplicationPage {
        public Settings() {
            InitializeComponent();
            var t = PimpViewModel.Instance.Limits.CalculateConsumedGb();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            DataContext = PimpViewModel.Instance;
            base.OnNavigatedTo(e);
        }
        private void EndpointsListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            WithSelection2<MusicEndpoint>(EndpointsListSelector, endpoint => {
                if (endpoint != EndpointsData.ThisDevice) {
                    GoToProjectPage("MusicPimp-WP", typeof(ConfigureEndpoint).Name, "?edit=" + Strings.encode(endpoint.Name));
                }
            });
        }
        protected void WithSelection2<T>(LongListSelector selector, Action<T> code) {
            try {
                if (selector.SelectedItem == null)
                    return;
                var selectedItem = (T)(selector.SelectedItem);
                code(selectedItem);
            } finally {
                selector.SelectedItem = null;
            }
        }
    }
}