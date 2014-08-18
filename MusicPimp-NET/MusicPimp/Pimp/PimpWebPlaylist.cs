using Mle.Concurrent;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.ViewModels;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Pimp {
    public class PimpWebPlaylist : WebSocketPlaylistBase {
        private PimpSessionBase session;

        public PimpWebPlaylist(PimpSessionBase session, PimpWebSocket webSocket)
            : base(webSocket) {
            this.session = session;
        }
        protected override Task SendSkipCommand(int playlistIndex) {
            return PostValue("skip", playlistIndex);
        }
        protected override Task RemoveSongInternal(int playlistIndex) {
            return PostValue("remove", playlistIndex);
        }
        public override Task SetPlaylist(MusicItem song) {
            return PostTrack("play", song);
        }
        public override Task AddSong(MusicItem song) {
            return PostTrack("add", song);
        }
        private Task PostTrack(string command, MusicItem song) {
            return session.PostTrack(command, song.Id, PimpWebPlayer.postResource);
        }
        private Task PostValue(string command, int value) {
            return session.PostValue(command, value, PimpWebPlayer.postResource);
        }
        public override Task LoadData() {
            return AsyncTasks.Noop();
        }
    }
}
