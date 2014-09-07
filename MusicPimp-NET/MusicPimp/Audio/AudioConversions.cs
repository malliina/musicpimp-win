using Mle.IO;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.Subsonic;
using Mle.MusicPimp.ViewModels;
using PCLWebUtility;
using System;

namespace Mle.MusicPimp.Audio {
    public class AudioConversions {
        public static MusicItem PimpTrackToMusicItem(PimpTrack track, Uri source, string username, string password) {
            string path = null;
            var maybeId = track.id;
            if(maybeId != null) {
                path = FileUtilsBase.UnixSeparators(WebUtility.UrlDecode(maybeId));
            }
            return new MusicItem() {
                Id = maybeId,
                Name = track.title,
                Artist = track.artist,
                Album = track.album,
                Path = path,
                Duration = TimeSpan.FromSeconds(track.duration),
                IsDir = false,
                Size = track.size,
                Source = source
            };
        }
        public static MusicItem FolderToMusicItem(PimpFolder folder) {
            return new MusicItem() {
                Id = folder.id,
                Name = folder.title,
                Path = FileUtilsBase.UnixSeparators(WebUtility.UrlDecode(folder.id)),
                IsDir = true
            };
        }
        public static MusicItem EntryToMusicItem(Entry track, Uri uri, string username, string password) {
            if(track.isDir) {
                return DirEntryToMusicItem(track);
            } else {
                return SongEntryToMusicItem(track, uri, username, password);
            }
        }
        public static MusicItem SongEntryToMusicItem(Entry e, Uri uri, string username, string password) {
            return new MusicItem() {
                Id = "" + e.id,
                Name = e.title,
                Album = e.album,
                Artist = e.artist,
                Path = e.path,
                IsDir = e.isDir,
                Size = e.size,
                Duration = TimeSpan.FromSeconds(e.duration),
                Source = uri
            };
        }
        private static MusicItem DirEntryToMusicItem(Entry dir) {
            return new MusicItem() {
                Id = "" + dir.id,
                Name = dir.title,
                IsDir = dir.isDir
            };
        }
        public static MusicItem artist2musicItem(Artist artist) {
            return new MusicItem() {
                Id = "" + artist.id,
                Name = artist.name,
                IsDir = true
            };
        }
        public static MusicItem playTrack2musicItem(PlaylistTrack track, string username, string password) {
            return new MusicItem() {
                Id = track.Path,
                Name = track.Title,
                Artist = track.Artist,
                Album = track.Album,
                Path = track.Path,
                IsDir = false,
                Source = track.Source,
                Size = track.Size,
                Duration = TimeSpan.FromSeconds(track.DurationSeconds)
            };
        }
        public static PlaylistTrack ToPlaylistTrack(MusicItem song) {
            return new PlaylistTrack(
                song.Source,
                song.Name,
                song.Artist,
                song.Album,
                song.Path,
                song.Size,
                song.Duration.TotalSeconds);
        }
    }
}
