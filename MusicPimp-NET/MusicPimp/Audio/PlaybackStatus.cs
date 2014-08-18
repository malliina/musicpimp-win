using Mle.MusicPimp.ViewModels;
using System;
using System.Collections.Generic;

namespace Mle.MusicPimp.Audio {
    /// <summary>
    /// A snapshot of a player.
    /// </summary>
    public class PlaybackStatus {
        public MusicItem Track { get; private set; }
        public TimeSpan TrackPosition { get; private set; }
        public int PlaylistIndex { get; private set; }
        public IEnumerable<MusicItem> Playlist { get; private set; }
        public int Volume { get; private set; }
        public PlayerState State { get; private set; }
        public bool IsMute { get; private set; }

        public PlaybackStatus(
            MusicItem track,
            TimeSpan pos,
            int index,
            IEnumerable<MusicItem> playlist,
            int vol,
            PlayerState state,
            bool isMute) {
            Track = track;
            TrackPosition = pos;
            PlaylistIndex = index;
            Playlist = playlist;
            Volume = vol;
            State = state;
            IsMute = isMute;
        }

    }
}
