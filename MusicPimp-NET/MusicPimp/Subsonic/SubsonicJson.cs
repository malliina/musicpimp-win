using Mle.MusicPimp.Audio;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

// classes generated with json2csharp.com and slightly modified
namespace Mle.MusicPimp.Subsonic {
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">type of response inside "subsonic-response" json</typeparam>
    abstract public class ISubsonicResponseContainer<T>
    where T : SubsonicResponse {
        // the dash in the JSON response breaks things unless the following line is included
        [JsonProperty(PropertyName = "subsonic-response")]
        public T subsonicResponse { get; set; }
    }
    public class SubsonicResponseContainer : ISubsonicResponseContainer<SubsonicResponse> { }
    public class SubsonicIndexesContainer : ISubsonicResponseContainer<IndexesResponse> { }
    public class SubsonicDirectoryContainer : ISubsonicResponseContainer<DirectoryResponse> { }
    public class JukeboxControlContainer : ISubsonicResponseContainer<JukeboxPlaylistResponse> { }
    public class JukeboxStatusContainer : ISubsonicResponseContainer<JukeboxStatusResponse> { }
    public class SearchContainer : ISubsonicResponseContainer<SearchResponse> { }
    public class PlaylistsContainer : ISubsonicResponseContainer<PlaylistsResponse> { }
    public class PlaylistContainer : ISubsonicResponseContainer<PlaylistResponse> { }
    public class GenericSubsonicContainer<T> : ISubsonicResponseContainer<T> where T : SubsonicResponse { }
    public class Error {
        public string message { get; set; }
        public int code { get; set; }
    }

    public class SubsonicResponse {
        public Error error { get; set; }
        public string status { get; set; }
        public string xmlns { get; set; }
        public string version { get; set; }
        public override string ToString() {
            return "status: " + status + ", version: " + version + ", xmlns: " + xmlns;
        }
    }
    public class SearchResponse : SubsonicResponse {
        [JsonConverter(typeof(MaybeSearchResultsConverter))]
        public SearchResult searchResult2 { get; set; }
    }
    public class SearchResult {
        [JsonConverter(typeof(ListOrNoListJsonConverter<Entry>))]
        public List<Entry> song { get; set; }
    }

    public class IndexesResponse : SubsonicResponse {
        public Indexes indexes { get; set; }
    }

    public class Artist {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Index {
        public string name { get; set; }
        [JsonConverter(typeof(ListOrNoListJsonConverter<Artist>))]
        public List<Artist> artist { get; set; }
    }

    public class Indexes {
        [JsonProperty(PropertyName = "child")]
        [JsonConverter(typeof(ListOrNoListJsonConverter<Entry>))]
        public List<Entry> child { get; set; }
        [JsonConverter(typeof(ListOrNoListJsonConverter<Index>))]
        public List<Index> index { get; set; }
        public long lastModified { get; set; }
    }
    public class Directory {
        [JsonProperty(PropertyName = "child")]
        [JsonConverter(typeof(ListOrNoListJsonConverter<Entry>))]
        public List<Entry> child { get; set; }
        public int id { get; set; }
        public string name { get; set; }
    }
    public class Entry {
        public string genre { get; set; }
        public string album { get; set; }
        public int parent { get; set; }
        public bool isDir { get; set; }
        public string contentType { get; set; }
        public string type { get; set; }
        public string suffix { get; set; }
        public bool isVideo { get; set; }
        public int size { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public int duration { get; set; }
        public string created { get; set; }
        public string path { get; set; }
        public string artist { get; set; }
        public int bitRate { get; set; }
        public int? albumId { get; set; }
        public int? track { get; set; }
        public int? discNumber { get; set; }
        public int? artistId { get; set; }
        public int? year { get; set; }
    }

    public class JukeboxPlaylist : JukeboxStatus {
        [JsonConverter(typeof(ListOrNoListJsonConverter<Entry>))]
        public List<Entry> entry { get; set; }
    }

    public class JukeboxPlaylistResponse : SubsonicResponse {
        public JukeboxPlaylist jukeboxPlaylist { get; set; }
    }
    public class JukeboxStatus {
        public int position { get; set; }
        public bool playing { get; set; }
        public int currentIndex { get; set; }
        public double gain { get; set; }
    }

    public class JukeboxStatusResponse : SubsonicResponse {
        public JukeboxStatus jukeboxStatus { get; set; }
    }
    public class DirectoryResponse : SubsonicResponse {
        public Directory directory { get; set; }
    }
    public class PlaylistsResponse : SubsonicResponse {
        [JsonConverter(typeof(MaybePlaylistsConverter))]
        public Playlists playlists { get; set; }
    }
    public class Playlists {
        [JsonConverter(typeof(ListOrNoListJsonConverter<SavedPlaylistMeta>))]
        public List<SavedPlaylistMeta> playlist { get; set; }
    }
    public class PlaylistResponse : SubsonicResponse {
        public Playlist playlist { get; set; }
    }
    public class Playlist {
        private List<Entry> _entry = new List<Entry>();
        [JsonConverter(typeof(ListOrNoListJsonConverter<Entry>))]
        public List<Entry> entry {
            get { return _entry; }
            set { if(value != null) _entry = value; }
        }
    }
    public class MaybeSearchResultsConverter : ItemOrEmptyStringConverter<SearchResult> {
        public MaybeSearchResultsConverter() : base(new SearchResult() { song = new List<Entry>() }) { }
    }
    /// <summary>
    /// If the "playlists" key is an empty string, parses it as if there's a JSON object with an empty "playlist" array.
    /// 
    /// Example input:
    /// 
    /// "playlists": {"playlist": [
    /// "playlists": ""
    /// 
    /// </summary>
    public class MaybePlaylistsConverter : ItemOrEmptyStringConverter<Playlists> {
        public MaybePlaylistsConverter() : base(new Playlists() { playlist = new List<SavedPlaylistMeta>() }) { }
    }
    /// <summary>
    /// Parses the JSON value as T if it's a non-empty string, or as defaultValue if it's the empty string.
    /// 
    /// Subsonic may return a JSON object with some collection of items under some key if there are any results, 
    /// but simply an empty string if there are no results. This is inconvenient and I do not want to fuck around 
    /// with nulls. If an empty string is encountered where normally more is expected, we parse it as defaultValue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ItemOrEmptyStringConverter<T> : CustomJsonConverterBase<T> {
        private T defaultValue;
        public ItemOrEmptyStringConverter(T defaultValue) {
            this.defaultValue = defaultValue;
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if(reader.TokenType == JsonToken.String && reader.Value == String.Empty) {
                return defaultValue;
            } else {
                return serializer.Deserialize<T>(reader);
            }
        }
    }
    public class ListOrNoListJsonConverter<T> : CustomJsonConverterBase<List<T>> {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            // Always reads a list
            if(reader.TokenType == JsonToken.StartArray) {
                // Json contains an array of entries ...
                return serializer.Deserialize<List<T>>(reader);
            } else if(reader.TokenType == JsonToken.String && reader.Value == String.Empty) {
                return new List<T>();
            } else {
                // ... or just a single entry (no array)
                T item = serializer.Deserialize<T>(reader);
                return new List<T>(new[] { item });
            }
        }
    }
    public abstract class CustomJsonConverterBase<T> : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof(T).IsAssignableFrom(objectType);
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

    }

}
