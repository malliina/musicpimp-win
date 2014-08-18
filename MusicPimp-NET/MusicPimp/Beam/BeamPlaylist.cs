using Mle.Concurrent;
using Mle.Exceptions;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.Util;
using Mle.MusicPimp.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Beam {


    public class BeamPlaylist : TimeBasedPlaylist {
        protected PimpSession session;
        private BeamPlayer player;
        private LibraryManager LibraryManager {
            get { return ProviderService.Instance.LibraryManager; }
        }
        private LocalMusicLibrary LocalLibrary {
            get { return ProviderService.Instance.LocalLibrary; }
        }

        public BeamPlaylist(PimpSession session, PimpWebSocket webSocket, BeamPlayer player)
            : base(webSocket) {
            this.session = session;
            this.player = player;
        }
        /// <summary>
        /// Opportunistically attempts to find out whether this player can 
        /// successfully upload a track to MusicBeamer. Success is not 
        /// guaranteed even if this validation passes, but uploading a
        /// track in vain is expensive due to problems with Play! Framework 
        /// file uploads: the whole track may be uploaded only to eventually 
        /// be rejected, so we minimize those cases by asking nicely if the
        /// playback device appears ready to receive a track.
        /// </summary>
        /// <returns>true if it is ok to upload a track to musicbeamer</returns>
        private async Task<bool> CanModifyPlaylist(bool validateAddAlso = false) {
            if(!webSocket.Socket.IsConnected) {
                Send("The MusicBeamer connection has failed. Check your settings.");
                return false;
            }
            var status = await session.ToJson<StreamableStatus>("/streamable");
            var canSet = CanSetPlaylist(status);
            return validateAddAlso ? canSet && CanAddToPlaylist(status) : canSet;
        }
        private bool CanSetPlaylist(StreamableStatus status) {
            if(!status.exists) {
                Send("The playback device has disconnected. Please choose another playback device.");
            }
            return status.exists;
        }
        private bool CanAddToPlaylist(StreamableStatus status) {
            if(!status.ready) {
                Send("The playback device is still receiving another track. This action cannot be completed right now, please try again later.");
            }
            return status.ready;
        }
        public override async Task SetPlaylist(MusicItem song) {
            if(await CanModifyPlaylist()) {
                Songs.Clear();
                await OnUiThread(() => player.NowPlaying = song);
                // updates local playlist first; assumes upload is successful
                await Add(song);
                await Stream(song, "/stream");
            }
        }

        public override async Task AddSong(MusicItem song) {
            if(await CanModifyPlaylist(validateAddAlso: true)) {
                try {
                    await Add(song);
                    await Stream(song, "/stream/tail");
                } catch(BadRequestException) {
                    Send("Unable to add " + song.Name + " to the playlist. Most likely another track is concurrently being transferred to the same player. Please try again later.");
                }
            }
        }
        /// <summary>
        /// Streams the song to the destination HTTP resource on the MusicBeamer server.
        /// 
        /// The streaming is delegated to the active music library: if MusicPimp is 
        /// the library, the MusicPimp server performs the upload, provided it has the 
        /// track. If the track only exists locally or if the user has explicitly set
        /// the music source to local only, then this device uploads the track. If
        /// Subsonic is the music source, an error message is displayed to the user
        /// because Subsonic does not support uploading.
        /// </summary>
        /// <param name="song"></param>
        /// <param name="resource">HTTP resource on http://beam.musicpimp.org to stream to; either "/stream" or "/stream/tail"</param>
        /// <returns></returns>
        private async Task Stream(MusicItem song, string resource) {
            await WithOOMGuard(async () => await LibraryManager.MusicProvider.Upload(song, resource, session));
        }
        public void SetPlaylist(List<MusicItem> tracks) {
            for(int i = 0; i < tracks.Count; ++i) {
                var track = tracks[i];
                if(i == 0) {
                    SetPlaylist(track);
                } else {
                    AddSong(track);
                }
            }
        }
        public override Task SkipTo(int playlistIndex) {
            return player.seekToIndex(playlistIndex);
        }
        public double GetStartPosition(int playlistIndex) {
            if(playlistIndex < Songs.Count && playlistIndex >= 0) {
                return Songs.Take(playlistIndex).Select(s => s.Song.Duration.TotalSeconds).Sum();
            } else {
                return NO_POSITION;
            }
        }
        protected virtual Task Add(MusicItem song) {
            Songs.Add(new PlaylistMusicItem(song, Songs.Count));
            return AsyncTasks.Noop();
        }
        private Task PostValue(string command, int value) {
            return session.PostValue(command, value, PimpWebPlayer.postResource);
        }
        public override Task LoadData() {
            return AsyncTasks.Noop();
        }
        protected override Task SendSkipCommand(int playlistIndex) {
            // not used because SkipTo is overridden, fix api perhaps
            return AsyncTasks.Noop();
        }
        protected override Task RemoveSongInternal(int playlistIndex) {
            // not supported
            return AsyncTasks.Noop();
        }
    }
    public class StreamableStatus {
        public string user { get; set; }
        public bool exists { get; set; }
        public bool ready { get; set; }
    }
}
