using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Network;
using System.Collections.Generic;
using System.Linq;

namespace Mle.MusicPimp.Beam {
    /// <summary>
    /// Syncs playlist based on websocket events from the player.
    /// </summary>
    public abstract class TimeBasedPlaylist : BasePlaylist {

        protected PimpWebSocket webSocket;

        public double StartedFromSeconds { get; set; }

        public TimeBasedPlaylist(PimpWebSocket webSocket) {
            this.webSocket = webSocket;
            StartedFromSeconds = 0;
            AutoPlay = true;
            webSocket.PlaylistModified += Sync;
            webSocket.PlaylistIndexChanged += index => Index = index;
            webSocket.TimeUpdated += webSocket_TimeUpdated;
        }

        private void webSocket_TimeUpdated(double time) {
            Index = GetIndex(time);
        }
        /// <summary>
        /// Determines the playlist index by comparing the combined playback time
        /// with the duration of the tracks in the playlist.
        /// 
        /// For example, if there are three tracks with duration 30s, 40s and 50s in
        /// the playlist, and the current time is 35s, then the playlist is playing the
        /// second track.
        /// 
        /// TODO: Is this accurate?
        /// 
        /// TODO2: Consider throwing exception instead of NO_POSITION
        /// </summary>
        /// <param name="time"></param>
        /// <returns>the playlist index or NO_POSITION if it could not be determined</returns>
        public int GetIndex(double time) {
            List<double> durations = Songs.Select(s => s.Song.Duration.TotalSeconds).ToList();
            double acc = 0;
            var durCount = durations.Count;
            for (int i = 0; i < durCount; ++i) {
                acc += durations[i];
                if (time + StartedFromSeconds < acc) {
                    return i;
                }
            }
            return NO_POSITION;
        }
        public double GetTrackTime(double streamTime) {
            var idx = GetIndex(streamTime);
            var trackStartTime = Songs.Take(idx).Select(s => s.Song.Duration.TotalSeconds).Sum();
            return streamTime - trackStartTime;
        }
    }
}
