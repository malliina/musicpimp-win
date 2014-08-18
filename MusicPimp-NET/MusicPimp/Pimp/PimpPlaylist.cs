using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Pimp {
    public class PimpPlaylist : SimplePimpPlaylist {
        private string playlistResource = "/playlist";
        private Func<MusicEndpoint> musicSource;
        private PimpSession s;

        public PimpPlaylist(PimpSession session, Func<MusicEndpoint> musicSource, PimpWebSocket webSocket)
            : base(session, webSocket) {
            this.s = session;
            this.musicSource = musicSource;
        }
        /// <summary>
        /// Determines whether the music source also functions as the playback device,
        /// in which case no file transfers are needed.
        /// </summary>
        /// <returns>true if both the music source and playback device are the same endpoint, false otherwise</returns>
        private bool IsSourceAlsoPlayer() {
            var source = musicSource();
            return session.Server == source.Server && session.Port == source.Port;
        }

        public override async Task AddSong(MusicItem song) {
            if(IsSourceAlsoPlayer()) {
                await base.AddSong(song);
            } else {
                await Upload(song, playlistResource + "/uploads");
            }
        }

        public override async Task SetPlaylist(MusicItem song) {
            if(IsSourceAlsoPlayer()) {
                await base.SetPlaylist(song);
            } else {
                await Upload(song, "/playback/server");
            }
        }
        private Task Upload(MusicItem song, string resource) {
            return WithOOMGuard(async () => await s.Upload(song, resource));
        }
    }
}
