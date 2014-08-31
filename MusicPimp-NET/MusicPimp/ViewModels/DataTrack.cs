
using Newtonsoft.Json;
namespace Mle.MusicPimp.ViewModels {
    public class DataTrack {
        public string Id { get; private set; }
        public string Artist { get; private set; }
        public string Album { get; private set; }
        public string Track { get; private set; }
        public DataTrack(string id, string artist, string album, string track) {
            Id = id;
            Artist = artist;
            Album = album;
            Track = track;
        }
    }
    public class DataTrackJson {
        public string id { get; set; }
        public string artist { get; set; }
        public string album { get; set; }
        public string track { get; set; }
        public DataTrack ToTrack() { return new DataTrack(id, artist, album, track); }
    }
}
