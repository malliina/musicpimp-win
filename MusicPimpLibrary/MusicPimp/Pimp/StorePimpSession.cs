using Mle.Audio;
using Mle.MusicPimp.Beam;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.ViewModels;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Pimp {
    public class StorePimpSession : PimpSession {

        public StorePimpSession(MusicEndpoint e)
            : base(e) {
        }
        protected override LocalMusicLibrary LocalLibrary {
            get { return MultiFolderLibrary.Instance; }
        }
        public override IDownloader BackgroundDownloader {
            get { return PimpStoreDownloader.Instance; }
        }
        protected override async Task EnsureHasDuration(MusicItem track) {
            var uri = await LocalLibrary.LocalUriIfExists(track);
            if(uri != null) {
                track.Duration = await StoreBeamPlayer.MediaElement.UriDuration(uri);
            }
        }

    }
}
