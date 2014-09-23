using Microsoft.Phone.Controls;
using Mle.MusicPimp.ViewModels;
using System.Windows.Navigation;

namespace Mle.MusicPimp.Xaml {
    public partial class Playlists : PhoneApplicationPage {
        private PlaylistsVM vm = new PlaylistsVM();
        public Playlists() {
            InitializeComponent();
            DataContext = vm;
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            await vm.Update();
        }

        private void PlaylistSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            
        }
    }
}