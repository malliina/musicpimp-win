using Mle.MusicPimp.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mle.MusicPimp.Database {
    public class PlaylistDatabase {
        public static PlaylistInfo TracksAndIndex() {
            return MusicDataContext.WithConnection<PlaylistInfo>(db => {
                var tracks = Tracks(db);
                return new PlaylistInfo(tracks, Index(db));
            });
        }
        public static List<PlaylistTrack> Tracks() {
            return MusicDataContext.WithConnection<List<PlaylistTrack>>(db => {
                return Tracks(db);
            });
        }
        public static PlaylistTrack Next() {
            return MusicDataContext.WithSubmit(db => {
                var index = Index(db);
                var tracks = Tracks(db);
                if(tracks.Count > ++index) {
                    var track = tracks[index];
                    SetIndex(db, index);
                    return track;
                } else {
                    return null;
                }
            });
        }
        public static PlaylistTrack Previous() {
            return MusicDataContext.WithSubmit(db => {
                var index = Index(db);
                var tracks = Tracks(db);
                if(index > 0 && tracks.Count > --index) {
                    var track = tracks[index];
                    SetIndex(db, index);
                    return track;
                } else {
                    return null;
                }
            });
        }
        private static List<PlaylistTrack> Tracks(MusicDataContext db) {
            var allSongs = from PlaylistItem song in db.Playlist orderby song.ItemId select song;
            if(allSongs.Any()) {
                // throws OOM upon "next track" following eom
                return new List<PlaylistTrack>(allSongs.Select(item2track));
            } else {
                return new List<PlaylistTrack>();

            }
        }
        public static int Index() {
            return MusicDataContext.WithConnection<int>(db => {
                return Index(db);
            });
        }
        private static int Index(MusicDataContext db) {
            var entry = (from PlaylistIndex i in db.PlayIndex select i).SingleOrDefault();
            if(entry != null)
                return entry.Index;
            else
                return -1;

        }
        public static void SetIndex(int newIndex) {
            MusicDataContext.WithSubmit(db => {
                SetIndex(db, newIndex);
            });
        }
        private static void SetIndex(MusicDataContext db, int newIndex) {
            var entry = (from PlaylistIndex i in db.PlayIndex select i);
            if(entry.Any()) {
                foreach(PlaylistIndex pi in entry) {
                    pi.Index = newIndex;
                }
            } else {
                var newEntry = new PlaylistIndex() { Index = newIndex };
                db.PlayIndex.InsertOnSubmit(newEntry);
            }
        }
        public static void Clear() {
            MusicDataContext.WithSubmit(db => {
                Clear(db);
            });
        }
        private static void Clear(MusicDataContext db) {
            var allSongs = from PlaylistItem song in db.Playlist select song;
            db.Playlist.DeleteAllOnSubmit(allSongs);
        }
        public static void SetPlaylist(PlaylistTrack track) {
            MusicDataContext.WithSubmit(db => {
                Clear(db);
                Add(db, track);
                SetIndex(db, 0);
            });
        }
        public static void Add(PlaylistTrack track) {
            AddAll(new List<PlaylistTrack>() { track });
        }
        public static void AddAll(IEnumerable<PlaylistTrack> tracks) {
            MusicDataContext.WithSubmit(db => {
                Add(db, tracks);
            });
        }
        private static void Add(MusicDataContext db, PlaylistTrack track) {
            Add(db, new List<PlaylistTrack>() { track });
        }
        private static void Add(MusicDataContext db, IEnumerable<PlaylistTrack> tracks) {
            var entries = tracks.Select(track2item);
            db.Playlist.InsertAllOnSubmit(entries);
        }
        /// <summary>
        /// http://stackoverflow.com/questions/2801500/linq-display-row-numbers
        /// </summary>
        /// <param name="index"></param>
        public static void Delete(int index) {
            MusicDataContext.WithSubmit(db => {
                var songs = (from PlaylistItem song in db.Playlist orderby song.ItemId select song)
                    .AsEnumerable();
                if(songs != null && songs.Count() > index) {
                    var song = songs.ElementAt(index);
                    db.Playlist.DeleteOnSubmit(song);
                }
            });
        }
        private static PlaylistTrack item2track(PlaylistItem item) {
            return new PlaylistTrack(
                new Uri(item.Source, UriKind.RelativeOrAbsolute),
                item.Title,
                item.Artist,
                item.Album,
                item.Path,
                item.Size,
                item.DurationSeconds);
        }
        private static PlaylistItem track2item(PlaylistTrack item) {
            return new PlaylistItem() {
                Source = item.Source.OriginalString,
                Title = item.Title,
                Artist = item.Artist,
                Album = item.Album,
                Path = item.Path,
                Size = item.Size,
                DurationSeconds = item.DurationSeconds
            };
        }

    }
}
