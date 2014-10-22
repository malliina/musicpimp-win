using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;

namespace Mle.MusicPimp.Beam {
    public class StoreBeamSession : StorePimpSession {
        public StoreBeamSession(MusicEndpoint settings)
            : base(settings) {
            SocketResource = BeamPlayer.webSocketResource;
        }
    }
}
