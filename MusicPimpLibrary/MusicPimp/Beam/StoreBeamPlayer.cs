using Mle.Audio;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.Tiles;
using Mle.MusicPimp.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Mle.MusicPimp.Beam {
    public class StoreBeamPlayer : BeamPlayer {

        // not used for playback, only duration calculation, defined in App.xaml
        private static MediaElement mediaElement;
        public static MediaElement MediaElement {
            get {
                if(mediaElement == null) {
                    // this should only be called after a page has been initialized. 
                    // has that happened here for sure?
                    var rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);
                    mediaElement = (MediaElement)VisualTreeHelper.GetChild(rootGrid, 1);
                }
                return mediaElement;
            }
        }

        public override BeamPlaylist BeamPlaylist { get; protected set; }

        public override BasePlaylist Playlist { get; protected set; }

        public StoreBeamPlayer(PimpSession session, PimpWebSocket webSocket)
            : base(session, webSocket, StoreCoverService.Instance) {
            BeamPlaylist = new StoreBeamPlaylist(session, webSocket, this);
            Init();
        }
        protected override async Task EnsureHasDuration(MusicItem track) {
            var maybeLocalUri = await MultiFolderLibrary.Instance.LocalUriIfExists(track);
            var uri = maybeLocalUri == null ? track.Source : maybeLocalUri;
            track.Duration = await MediaElement.UriDuration(uri);
        }
    }
}
