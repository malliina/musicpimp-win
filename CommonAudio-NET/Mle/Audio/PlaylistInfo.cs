using System.Collections.Generic;
using System.Linq;

namespace Mle.MusicPimp.Audio {
    public class PlaylistInfo {
        public List<PlaylistTrack> Tracks { get; private set; }
        public int CurrentIndex { get; set; }

        public PlaylistInfo(IEnumerable<PlaylistTrack> tracks, int currentIndex) {
            Tracks = tracks.ToList<PlaylistTrack>();
            CurrentIndex = currentIndex;
        }

        public PlaylistInfo(PlaylistTrack track) : this(new List<PlaylistTrack>() { track }, 0) { }

        public PlaylistInfo() : this(new List<PlaylistTrack>(), 0) { }

        public static PlaylistInfo Empty() {
            return new PlaylistInfo(new List<PlaylistTrack>(), 0);
        }
    }
}
