using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.Tiles;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Beam {
    public abstract class BeamPlayer : WebSocketPlayer {
        public static readonly string webSocketResource = "/ws/control";

        public abstract BeamPlaylist BeamPlaylist { get; protected set; }

        protected abstract Task EnsureHasDuration(MusicItem track);

        public BeamPlayer(PimpSession session, PimpWebSocket webSocket, CoverService coverService)
            : base(session, webSocket) {
            IsSkipAndSeekSupported = false;

            Volume = 100;
            IsMute = false;

            webSocket.Welcomed += webSocket_Welcomed;
            webSocket.Disconnected += webSocket_Disconnected;

            TrackChanged += BeamPlayer_TrackChanged;
        }

        private async void BeamPlayer_TrackChanged(MusicItem item) {
            if(item != null) {
                var ev = new TrackChangedEvent() {
                    Event = "track_changed",
                    track = new PimpTrack(item)
                };
                await Send(ev);
            }
        }
        // not in constructor because BeamPlaylist is created in subclass so it's called from there
        protected void Init() {
            Playlist = BeamPlaylist;
            Playlist.IndexChanged += SetTrackFromIndex;
            Playlist.Songs.CollectionChanged += Songs_CollectionChanged;
        }
        protected virtual async void Songs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            await Utils.SuppressAsync<Exception>(async () => {
                SetTrackFromIndex(Playlist.Index);
                await EnsureSongsHaveDuration(Playlist.Songs);
            });
        }
        /// <summary>
        /// Searches for zero-duration songs in the playlist and attempts to resolve the duration
        /// if any are found.
        /// 
        /// The beam player needs the duration of each song in order to determine the position
        /// in the playlist during stream playback.
        /// </summary>
        /// <param name="playlistItems">items in the playlist</param>
        /// <returns></returns>
        private async Task EnsureSongsHaveDuration(IList<PlaylistMusicItem> playlistItems) {
            var zeroDurationSongs = playlistItems
                .Select(s => s.Song)
                .Where(s => s.Duration.TotalSeconds == 0)
                .ToList();

            foreach(var track in zeroDurationSongs) {
                await EnsureHasDuration(track);
            }
        }
        private void webSocket_Disconnected(string user) {
            if(session.Username == user) {
                Send("MusicBeamer disconnected", "The MusicBeamer endpoint disconnected. Attempts to control playback will likely fail. You might want to adjust your player settings.");
            }
        }

        private async void webSocket_Welcomed() {
            // Informs the web browser, via the beam server, that a mobile device has successfully connected.
            // After the reception of this "connected" message, the browser can hide the QR code and display
            // the audio player as playback is likely to commence.
            await PostSimple("connected");
        }
        public override Task HandleToggleMute(bool newMuteValue) {
            return Post("mute", newMuteValue);
        }
        public override void OnTimeUpdated(double time) {
            TrackPosition = TimeSpan.FromSeconds(BeamPlaylist.GetTrackTime(time));
        }
        protected void SetTrackFromIndex(int index) {
            if(index >= 0 && index < Playlist.Songs.Count) {
                OnUiThread(() => NowPlaying = Playlist.Songs[index].Song);
            }
        }
        public override Task play() {
            return PostSimple("resume");
        }
        public override Task next() {
            return seekToIndex(Playlist.Index + 1);
        }
        public override Task previous() {
            return seekToIndex(Playlist.Index - 1);
        }
        /// <summary>
        /// Seeks to the given playlist index.
        /// 
        /// The start time of the track, in seconds, is determined, then
        /// a seek is done to that position. 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task seekToIndex(int index) {
            var startPosition = BeamPlaylist.GetStartPosition(index);
            if(startPosition != BasePlaylist.NO_POSITION) {
                await seek(startPosition);
            }
        }
        public override Task seek(double pos) {
            var startPosition = BeamPlaylist.GetStartPosition(Playlist.Index);
            return base.seek(startPosition + pos);
        }
        public override void UpdateStatus(PlaybackStatus status) {
        }
        public override Task<PlaybackStatus> Status() {
            return TaskEx.FromResult(new PlaybackStatus(
                 NowPlaying,
                 TrackPosition,
                 Playlist.Index,
                 Playlist.Songs.Select(s => s.Song).ToList(),
                 Volume,
                 CurrentPlayerState,
                 IsMute));
        }
    }
}
