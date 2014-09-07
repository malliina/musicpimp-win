using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Pimp {
    public abstract class PimpBasePlayer : WebSocketPlayer {
        public abstract Task<StatusPimpResponse> GetStatusAsync();

        public PimpBasePlayer(SimplePimpSession session, PimpWebSocket pimpSocket)
            : base(session, pimpSocket) {
        }

        public override Task HandleToggleMute(bool newMuteValue) {
            return Post("mute", newMuteValue);
        }

        public static PlaybackStatus ToPlaybackStatus(StatusPimpResponse status) {
            var playlist = status.playlist
                .Select(track => AudioConversions.PimpTrackToMusicItem(track, null, null, null))
                .ToList();
            return new PlaybackStatus(
                fromStatus(status),
                position(status),
                status.index,
                playlist,
                volume(status),
                playerState(status),
                status.mute);
        }
        public override async Task<PlaybackStatus> Status() {
            StatusPimpResponse status = await GetStatusAsync();
            return ToPlaybackStatus(status);
        }

        private static bool hasTrack(StatusPimpResponse status) {
            return status.state != "NoMedia" && status.state != PlayerState.Closed.ToString();
        }
        private static MusicItem fromStatus(StatusPimpResponse status) {
            if(!hasTrack(status)) {
                return null;
            }
            return AudioConversions.PimpTrackToMusicItem(status.track, null, null, null);
        }
        private static TimeSpan position(StatusPimpResponse status) {
            if(!hasTrack(status)) {
                return TimeSpan.FromSeconds(0);
            } else {
                return TimeSpan.FromSeconds(status.position);
            }
        }
        private static int volume(StatusPimpResponse status) {
            return status.volume;
        }
        private static PlayerState playerState(StatusPimpResponse status) {
            return PimpWebSocket.FromName(status.state);
        }
    }
}
