using System;

namespace Mle.MusicPimp.Audio {
    /// <summary>
    /// ...
    /// </summary>
    public class PlaylistTrack {
        public Uri Source { get; private set; }
        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string Album { get; private set; }
        public string Path { get; private set; }
        public long Size { get; set; }
        public double DurationSeconds { get; set; }
        //public string Username { get; set; }
        //public string Password { get; set; }

        public PlaylistTrack(Uri source, string title, string artist, string album, string path, long sizeBytes, double durationSeconds) {
            Source = source;
            Title = title;
            Artist = artist;
            Album = album;
            Path = path;
            Size = sizeBytes;
            DurationSeconds = durationSeconds;
        }
    }
}
