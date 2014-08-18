using Mle.Concurrent;
using Mle.MusicPimp.Audio;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Local {
    public abstract class LocalBasePlayer : BasePlayer {
        public abstract PimpPlayerElement PlayerControl { get; }
        public override string NetworkStatus {
            get { return "local player"; }
        }
        public LocalBasePlayer() {
            IsOnline = false;
        }
        public override Task TryToConnect() {
            return AsyncTasks.Noop();
        }
        public override Task Subscribe() {
            return AsyncTasks.Noop();
        }

        public override async Task play() {
            if(await PlayerControl.HasTrack()) {
                await PlayerControl.Play();
            } else {
                // plays first item from playlist, if available
                if(Playlist.Songs.Count > 0) {
                    await Playlist.SkipTo(0);
                    await PlayerControl.SetTrack(Playlist.Songs.ElementAt(0).Song);
                }
            }
        }
        public override async Task playPlaylist() {
            await Playlist.Load();
            var i = Playlist.Index;
            var song = Playlist.Songs.ElementAt(i).Song;
            await OnUiThread(() => NowPlaying = song);
            await PlayerControl.SetTrack(song);
        }

        // media elements must be modified on the UI thread; do so if you choose to use TaskEx.Run(...)

        public override Task pause() {
            return PlayerControl.Pause();
        }
        public override Task SetVolume(int newVolume) {
            PlayerControl.Volume = 1.0 * newVolume / 100;
            Volume = newVolume;
            return AsyncTasks.Noop();
        }
        public override Task seek(double pos) {
            PlayerControl.Position = TimeSpan.FromSeconds(pos);
            return AsyncTasks.Noop();
        }
        public override async Task<PlaybackStatus> Status() {
            await Playlist.Load();
            var tracks = Playlist.Songs.Select(s => s.Song).ToList();
            var currentTrack = await PlayerControl.CurrentTrack();
            return new PlaybackStatus(
                          currentTrack,
                           PlayerControl.Position,
                           Playlist.Index,
                           tracks,
                           (int)(100.0 * PlayerControl.Volume),
                           CurrentPlayerState,
                           IsMute);
        }
    }
}
