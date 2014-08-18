using Mle.MusicPimp.Network;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using System;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Audio {
    public class PimpPlayer : PimpBasePlayer {
        public override BasePlaylist Playlist { get; protected set; }

        public static readonly string webSocketResource = "/ws/playback";
        public static readonly string postResource = "/playback";

        public PimpPlayer(SimplePimpSession s, PimpWebSocket webSocket, BasePlaylist playlist)
            : base(s, webSocket) {
            Playlist = playlist;
        }
        public PimpPlayer(PimpSession s, Func<MusicEndpoint> musicSource, PimpWebSocket webSocket)
            : this(s, webSocket, new PimpPlaylist(s, musicSource, webSocket)) {
            //Playlist = new PimpPlaylist(s, musicSource, webSocket);
        }
        public override async Task play() {
            // if no track is set, inits the player with a track from the playlist 
            // TODO remove this, this is the responsibility of MusicPimp
            if(!IsTrackAvailable && Playlist.Songs.Count > 0) {
                await Playlist.SkipTo(0);
            }
            await PostSimple("resume");
        }

        public override Task next() {
            return Playlist.SkipTo(Playlist.Index + 1);
        }
        public override Task previous() {
            return Playlist.SkipTo(Playlist.Index - 1);
        }

        public override Task<StatusPimpResponse> GetStatusAsync() {
            return session.StatusCall();
        }
    }
}
