using Microsoft.Phone.BackgroundAudio;
using Mle.Concurrent;
using Mle.MusicPimp.Audio;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Local {
    public class PhoneLocalPlayer : LocalBasePlayer {
        private static PhoneLocalPlayer instance = null;
        public static PhoneLocalPlayer Instance {
            get {
                if(instance == null)
                    instance = new PhoneLocalPlayer();
                return instance;
            }
        }
        public override BasePlaylist Playlist { get; protected set; }

        private BackgroundAudioPlayer player = BackgroundAudioPlayer.Instance;

        public override PimpPlayerElement PlayerControl {
            get { return PhonePlayerElement.Instance; }
        }

        protected PhoneLocalPlayer() {
            Playlist = new PhoneLocalPlaylist();
            player.PlayStateChanged += (s, e) => {
                UpdatePlayerState();
            };
            UpdatePlayerState();
            TrackChanged += async track => {
                if(track != null && track.IsSourceLocal && track.Duration.TotalSeconds == 0) {
                    track.Duration = await DurationHelper.GetDuration(track);
                }
            };
        }

        public override Task next() {
            player.SkipNext();
            return AsyncTasks.Noop();
        }
        public override Task previous() {
            player.SkipPrevious();
            return AsyncTasks.Noop();
        }
        public void UpdatePlayerState() {
            CurrentPlayerState = GetPlayerState(player.PlayerState);
        }
        public static PlayerState GetPlayerState(PlayState state) {
            switch(state) {
                case PlayState.Playing:
                    return PlayerState.Playing;
                case PlayState.Paused:
                    return PlayerState.Paused;
                case PlayState.Stopped:
                    return PlayerState.Stopped;
                case PlayState.BufferingStarted:
                    return PlayerState.Buffering;
                default:
                    return PlayerState.Other;
            }
        }
    }
}
