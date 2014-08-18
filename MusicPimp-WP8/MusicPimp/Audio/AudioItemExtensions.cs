using Microsoft.Phone.BackgroundAudio;
using System;
using System.Runtime.InteropServices;

namespace Mle.MusicPimp.Audio {
    public static class AudioItemExtensions {
        public static TimeSpan DurationOrElse(this AudioTrack track, TimeSpan orElse) {
            try {
                return track.Duration;
            } catch (COMException) {
                // AudioTrack.Duration throws COMException immediately after construction; 
                // perhaps for as long as it has not been initialized in the player yet
                return orElse;
            }
        }
    }
}
