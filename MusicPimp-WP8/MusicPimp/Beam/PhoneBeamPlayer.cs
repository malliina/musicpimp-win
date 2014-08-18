using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.Tiles;
using Mle.MusicPimp.ViewModels;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Beam {
    public class PhoneBeamPlayer : BeamPlayer {

        public override BasePlaylist Playlist { get; protected set; }

        public override BeamPlaylist BeamPlaylist { get; protected set; }

        public PhoneBeamPlayer(PimpSession session, PimpWebSocket webSocket)
            : base(session, webSocket, PhoneCoverService.Instance) {
            BeamPlaylist = new PhoneBeamPlaylist(session, webSocket, this);
            Init();
        }
        protected override Task EnsureHasDuration(MusicItem track) {
            return DurationHelper.SetDuration(track);
        }
    }
}
