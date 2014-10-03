using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Pimp {
    /// <summary>
    /// TODO the session is only used for the status call; implement request-response Task
    /// over websocket, then we can get rid of the session.
    /// </summary>
    public class SimplePimpPlaylist : WebSocketPlaylistBase {
        protected SimplePimpSession session;
        public SimplePimpPlaylist(SimplePimpSession session, PimpWebSocket webSocket)
            : base(webSocket) {
            this.session = session;
        }
        protected override Task RemoveSongInternal(int playlistIndex) {
            return Send(new GenericCommand<int>("remove", playlistIndex));
        }
        protected override Task SendSkipCommand(int index) {
            return Send(new GenericCommand<int>("skip", index));
        }
        public override Task SetPlaylist(MusicItem song) {
            return Send(new TrackCommand("play", song.Id));
        }
        public override Task AddSong(MusicItem song) {
            return Send(new TrackCommand("add", song.Id));
        }
        private Task Send(JsonContent cmd) {
            return webSocket.Send(cmd);
        }
        public override async Task LoadData() {
            var status = await session.StatusCall();
            var items = status.playlist.Select(item => AudioConversions.PimpTrackToMusicItem(item, null, session.Username, session.Password, session.CloudServerID)).ToList();
            Sync(items, status.index);
        }
    }
}
