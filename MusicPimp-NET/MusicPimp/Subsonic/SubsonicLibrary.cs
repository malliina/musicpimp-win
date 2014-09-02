using Mle.Concurrent;
using Mle.Messaging;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.Subsonic;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Audio {
    public class SubsonicLibrary : MusicLibrary {
        private SubsonicSession session;
        public SubsonicLibrary(SubsonicSession s) {
            session = s;
            RootFolderKey = "-1";
            RootEmptyMessage = "Successfully connected to Subsonic at " + session.Describe + ", however the music library is empty. Follow the Subsonic server documentation to add MP3s to the library. Refresh the library of this app when you're done.";
            var t = Utils.SuppressAsync<Exception>(Ping);
        }
        public override async Task<IEnumerable<MusicItem>> Search(string term) {
            var result = await session.Search(term, maxSearchResults);
            // searchResult2 is occasionally null - no idea why
            var items = result.searchResult2;
            if(items != null) {
                var songs = items.song;
                if(songs != null) {
                    return EntriesToMusicItem(songs);
                }
            }
            return new List<MusicItem>();
        }
        /// <summary>
        /// We cannot use the 'download' functionality of Subsonic, because it does not
        /// set the content-length header (at least in 4.6), so such downloads will fail
        /// on WP8.
        /// 
        /// Property track.Source, which is the default implementation, contains the URI to 
        /// 'stream' the track, which means it may be transcoded during transfer. The track
        /// is eventually anyway saved to storage according to its server file Path, so this 
        /// means that a track transcoded to .mp3 on the fly may be saved as a .flac. 
        /// 
        /// TODO: To fix, we need to investigate the content of the response to find out the 
        /// audio format if possible. Maybe the Content-Type response header helps?
        /// </summary>
        /// <param name="track"></param>
        /// <returns>the remote URI for downloading the track, no transcoding</returns>
        //public override Uri DownloadUriFor(MusicItem track) {
        //int id;
        //var success = int.TryParse(track.Id, out id);
        //if(success) {
        //    return session.DownloadUriFor(id);
        //} else {
        //    return base.DownloadUriFor(track);
        //}
        //}
        public override async Task Ping() {
            await session.pingAsync();
            IsOnline = true;
        }
        public override Task Upload(MusicItem song, string resource, PimpSession destSession) {
            MessagingService.Instance.Send("Sorry, MusicBeaming is not supported with Subsonic as music source. The music source must be a MusicPimp server or your local device.");
            return AsyncTasks.Noop();
        }
        /// <summary>
        /// loads the user-selected folder or the indexes if no folder is selected
        /// </summary>
        /// <param name="folderId">the selected folder identifier, or -1 if no folder is selected</param>
        /// <returns></returns>
        protected async override Task<IEnumerable<MusicItem>> LoadFolderAsync(string folderIdString) {
            var folderId = -1;
            try {
                folderId = int.Parse(folderIdString);
            } catch(FormatException) { }
            if(folderId >= 0) {
                return await LoadMusicDirectory(folderId);
            } else {
                return await LoadIndexes();
            }
        }
        private async Task<List<MusicItem>> LoadIndexes() {
            var indexesJson = await session.indexesAsync();
            return RootFolderToMusicItems(indexesJson);
        }
        private async Task<List<MusicItem>> LoadMusicDirectory(int folderId) {
            var musicFilesJson = await session.musicFilesAsync(folderId);
            return FolderToMusicItems(musicFilesJson);
        }
        /// <summary>
        /// called when data is available following an async call
        /// 
        /// response 1-1 indexes 1-M index 1-M artist
        /// </summary>
        /// <param name="response">the fetched data</param>
        private List<MusicItem> RootFolderToMusicItems(IndexesResponse response) {
            var ret = new List<MusicItem>();
            var indexes = response.indexes.index;
            if(indexes != null) {
                foreach(var i in indexes) {
                    foreach(var a in i.artist) {
                        ret.Add(AudioConversions.artist2musicItem(a));
                    }
                }
            }
            var songs = response.indexes.child;
            if(songs != null) {
                AddAll(songs, ret);
            }
            return ret;
        }
        /// <summary>
        /// called when data is available following an async call
        /// </summary>
        /// <param name="response">the fetched data</param>
        private List<MusicItem> FolderToMusicItems(DirectoryResponse response) {
            var ret = new List<MusicItem>();
            var songOrDirs = response.directory.child;
            if(songOrDirs != null) {
                AddAll(songOrDirs, ret);
            }
            return ret;
        }
        private void AddAll(List<Entry> entries, ICollection<MusicItem> dest) {
            foreach(var item in entries) {
                Uri uri = null;
                if(!item.isDir) {
                    uri = session.StreamUriFor(item.id);
                }
                dest.Add(AudioConversions.EntryToMusicItem(item, uri));
            }
        }
        private IEnumerable<MusicItem> EntriesToMusicItem(List<Entry> entries) {
            var ret = new List<MusicItem>();
            AddAll(entries, ret);
            return ret;
        }
    }

}
