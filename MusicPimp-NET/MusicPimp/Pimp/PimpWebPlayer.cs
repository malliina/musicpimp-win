using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Network;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Pimp {
    public class PimpWebPlayer : PimpBasePlayer {

        public static readonly string webSocketResource = "/ws/webplay";
        public static readonly string postResource = "/webplay";

        public override BasePlaylist Playlist { get; protected set; }

        public PimpWebPlayer(PimpSession session, PimpWebSocket webSocket)
            : base(session, webSocket) {
            Playlist = new PimpWebPlaylist(session, webSocket);
        }

        public override Task play() {
            return PostSimple("resume");
        }
        public override Task next() {
            return PostSimple("next");
        }
        public override Task previous() {
            return PostSimple("prev");
        }
        public override Task<StatusPimpResponse> GetStatusAsync() {
            return session.ToJson<StatusPimpResponse>("/webplay");
        }
    }
}
