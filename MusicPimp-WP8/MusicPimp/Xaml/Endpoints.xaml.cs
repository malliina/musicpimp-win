using Microsoft.Phone.Controls;
using Mle.MusicPimp.Controls;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MusicPimp.Xaml {
    public partial class Endpoints : AsyncPhoneApplicationPage {
        public Endpoints() {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            DataContext = PimpViewModel.Instance.EndpointsData;
            LockScreen.Instance.CheckIsAppProvider();
            base.OnNavigatedTo(e);
        }
        private void EndpointsListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selector = sender as LongListSelector;
            var item = selector.SelectedItem;
            try {
                if(item != null) {
                    var endpoint = item as MusicEndpoint;
                    if(endpoint != EndpointsData.ThisDevice) {
                        GoToProjectPage("MusicPimp-WP8", typeof(ConfigureEndpoint).Name, "?edit=" + Strings.encode(endpoint.Name));
                    }
                }
            } finally {
                selector.SelectedItem = null;
            }
        }
    }
}