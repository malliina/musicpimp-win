using Mle.MusicPimp.Audio;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Mle.MusicPimp.Local {
    public class StoreLocalPlayer : LocalBasePlayer {
        private static StoreLocalPlayer instance = null;
        public static StoreLocalPlayer Instance {
            get {
                if (instance == null)
                    instance = new StoreLocalPlayer();
                return instance;
            }
        }
        public override PimpPlayerElement PlayerControl {
            // todo add debug stmt
            get { return MediaPlayerElement.Instance; }
        }
        private RoamingPlaylist playlist;
        public override BasePlaylist Playlist { get; protected set; }

        private StoreLocalPlayer() {
            playlist = new RoamingPlaylist();
            Playlist = playlist;
        }
        /// <summary>
        /// Must be called only after the page has been loaded; see Loaded event of RootPage.
        /// </summary>
        public void InitPlayerListeners() {
            var player = MediaPlayerElement.Instance.Player;
            player.MediaEnded += async (s, ev) => await next();
            player.CurrentStateChanged += (s, ev) => {
                UpdatePlayerState();
            };
            UpdatePlayerState();
        }
        private void UpdatePlayerState() {
            CurrentPlayerState = GetPlayerState(MediaPlayerElement.Instance.Player.CurrentState);
        }
        private static PlayerState GetPlayerState(MediaElementState state) {
            switch (state) {
                case MediaElementState.Playing:
                    return PlayerState.Playing;
                case MediaElementState.Paused:
                    return PlayerState.Paused;
                case MediaElementState.Stopped:
                    return PlayerState.Stopped;
                case MediaElementState.Buffering:
                    return PlayerState.Buffering;
                case MediaElementState.Opening:
                    return PlayerState.Opening;
                case MediaElementState.Closed:
                    return PlayerState.Closed;
                default:
                    return PlayerState.Other;
            }
        }
        public override async Task next() {
            await skipToIndex(p => p.SkipNext());
        }
        public override async Task previous() {
            await skipToIndex(p => p.SkipPrevious());
        }
        private async Task skipToIndex(Func<RoamingPlaylist, Task<int>> stepFunc) {
            var idx = await stepFunc(playlist);
            if (idx >= 0) {
                await Playlist.Load();
                var songs = Playlist.Songs;
                if (idx < songs.Count) {
                    var song = songs.ElementAt(idx).Song;
                    await OnUiThread(() => NowPlaying = song);
                    await PlayerControl.SetTrack(song);
                }
            }
        }
    }
}
