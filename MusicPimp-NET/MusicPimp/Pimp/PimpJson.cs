using Mle.MusicPimp.ViewModels;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mle.MusicPimp.Pimp {
    public class PimpJson {
        public static readonly IList<string> DayNames = new List<string>(){
            "sun","mon","tue","wed","thu","fri","sat"
        };
    }
    public class JsonContent {
        public string Json() {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class SimpleCommand : JsonContent {
        public string cmd { get; set; }
        public SimpleCommand(string c) {
            cmd = c;
        }
    }
    public class ValidateAnid2Command : SimpleCommand {
        public string key { get; set; }
        public ValidateAnid2Command(string key)
            : base("validate") {
                this.key = key;
        }
    }

    public class GenericCommand<T> : SimpleCommand {
        public T value { get; set; }
        public GenericCommand(string command, T value)
            : base(command) {
            this.value = value;
        }
    }
    public class IdCommand : SimpleCommand {
        public string id { get; set; }
        public IdCommand(string command, string id)
            : base(command) {
            this.id = id;
        }
    }
    public class TrackCommand : SimpleCommand {
        public string track { get; set; }
        public TrackCommand(string command, string t)
            : base(command) {
            track = t;
        }
    }
    public class BeamCommand : JsonContent {
        public string track { get; set; }
        public string uri { get; set; }
        public string username { get; set; }
        public string password { get; set; }

        public BeamCommand(string track, string uri, string username, string password) {
            this.track = track;
            this.uri = uri;
            this.username = username;
            this.password = password;
        }
    }
    public class BeamTrack : JsonContent {
        public string CoverUri { get; set; }
    }
    public abstract class PimpMusicItem {
        public string id { get; set; }
        public string title { get; set; }
    }
    public class PimpFolder : PimpMusicItem { }

    public class PimpTrack : PimpMusicItem {
        public string artist { get; set; }
        public string album { get; set; }
        public long size { get; set; }
        public double duration { get; set; }

        public PimpTrack() {

        }
        public PimpTrack(MusicItem item) {
            id = item.Id;
            title = item.Name;
            artist = item.Artist;
            album = item.Album;
            size = item.Size;
            duration = item.Duration.TotalSeconds;
        }
    }
    public class StatusPimpResponse {
        public PimpTrack track { get; set; }
        public string state { get; set; }
        public double position { get; set; }
        public int volume { get; set; }
        public bool mute { get; set; }
        public List<PimpTrack> playlist { get; set; }
        public int index { get; set; }
    }
    public class FoldersPimpResponse {
        public List<PimpFolder> folders { get; set; }
        public List<PimpTrack> tracks { get; set; }
    }
    public class FailureResponse {
        public string reason { get; set; }
    }
    public class VersionResponse {
        public string version { get; set; }
    }
}
