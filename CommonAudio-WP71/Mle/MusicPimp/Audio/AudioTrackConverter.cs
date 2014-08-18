using Microsoft.Phone.BackgroundAudio;
using Mle.IO;

namespace Mle.MusicPimp.Audio {
    public class AudioTrackConverter {
        private static string MUSIC_HOME = "music/";

        public static AudioTrack ToTrack(PlaylistTrack track) {
            // uses offline source if available
            var maybeLocalPath = MUSIC_HOME + track.Path;
            var localUri = PhoneFileUtils.Instance.GetUri(maybeLocalPath, (ulong)track.Size);
            var preferredSource = localUri != null ? localUri : track.Source;

            return new AudioTrack(
                preferredSource,
                track.Title,
                track.Artist,
                track.Album,
                null,
                track.Path,
                EnabledPlayerControls.All);
        }
    }
}
