using Microsoft.Phone.Controls;
using Mle.MusicPimp.ViewModels;
using Mle.ViewModels;

namespace Mle.MusicPimp.Xaml {
    public partial class VolumeSlider : PhoneApplicationPage {

        public PhoneNowPlaying ViewModel {
            get { return PhoneNowPlaying.Instance; }
        }

        public VolumeSlider() {
            InitializeComponent();
            DataContext = ViewModel;
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            ViewModel.StartPollingIfNeeded();
        }
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e) {
            base.OnNavigatedFrom(e);
            ViewModel.StopPollingIfNeeded();
        }

        private async void VolumeSlider_LostCapture(object sender, System.Windows.Input.MouseEventArgs e) {
            var player = ViewModel.Player;
            player.IsMute = false;
            await player.SetVolume((int)volumeSlider.Value);
        }
    }
}