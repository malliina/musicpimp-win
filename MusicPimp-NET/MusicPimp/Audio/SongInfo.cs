
using System;
namespace Mle.MusicPimp.Audio {
    public class SongInfo {
        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string Album { get; private set; }
        public TimeSpan Duration { get; private set; }

        public SongInfo(string title, string artist, string album, TimeSpan duration) {
            Title = title;
            Artist = artist;
            Album = album;
            Duration = duration;
        }
    }
}
