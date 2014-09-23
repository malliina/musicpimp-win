using Mle.Concurrent;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Audio {
    /// <summary>
    /// A library that stores music folders in a dictionary. The audio source uniquely identifies each folder by a key. 
    /// Subclasses retrieve music folder information from the audio source using the key.
    /// </summary>
    public abstract class MusicLibrary : NetworkedViewModel {
        /// <summary>
        /// Triggers when folder from a sub-library (or the whole library) have been loaded. UIs may wish to update themselves at this point.
        /// </summary>
        public event Action<IEnumerable<MusicItem>> NewItemsLoaded;
        public Dictionary<string, IEnumerable<MusicItem>> Folders { get; private set; }
        public string RootFolderKey { get; protected set; }
        public virtual string RootEmptyMessage { get; protected set; }
        public abstract Task Ping();
        protected abstract Task<IEnumerable<MusicItem>> LoadFolderAsync(string id);
        public abstract Task<IEnumerable<MusicItem>> Search(string term);
        public abstract Task Upload(MusicItem song, string resource, PimpSession destSession);
        protected int maxSearchResults;
        public bool ServerSupportsPlaylists { get; protected set; }

        public MusicLibrary(int maxSearchResults = 100) {
            Folders = new Dictionary<string, IEnumerable<MusicItem>>();
            RootEmptyMessage = "the library is empty";
            RootFolderKey = String.Empty;
            this.maxSearchResults = maxSearchResults;
            ServerSupportsPlaylists = false;
        }
        public virtual Task<List<SavedPlaylistMeta>> LoadPlaylists() {
            return EmptyTaskList<SavedPlaylistMeta>();
        }
        public virtual Task<List<MusicItem>> LoadPlaylist(string playlistID) {
            return EmptyTaskList<MusicItem>();
        }
        public virtual Task DeletePlaylist(string playlistID) {
            return AsyncTasks.Noop();
        }
        private Task<List<T>> EmptyTaskList<T>() {
            return TaskEx.FromResult<List<T>>(new List<T>());
        }
        /// <summary>
        /// Non-recursively loads the user-selected folder or the root folder if no folder is selected.
        /// 
        /// Does not check whether the returned tracks are locally available, so LocalUri == null for all.
        /// 
        /// TODO Document what must happen if a folder with that ID does not exist? Throw exception?
        /// </summary>
        /// <param name="id">the folder identifier; the format is implementation-dependent</param>
        /// <param name="to">the place to put loaded music items</param>
        /// <returns>subfolders and tracks in the given folder</returns>
        public virtual async Task LoadFolderAsync2(string folderId, ObservableCollection<MusicItem> to) {
            var items = await LoadFolderAsync(folderId);
            AddDistinct(items, to);
        }
        public virtual string DirectoryIdentifier(MusicItem musicDir) {
            return musicDir.Id;
        }
        public virtual Uri DownloadUriFor(MusicItem track) {
            return track.Source;
        }
        /// <summary>
        /// It is sufficient for this method to only inspect the contents of the cache, 
        /// if any. It is allowed but not mandatory to perform remote operations.
        /// </summary>
        /// <param name="path">path to music item: a folder or a track</param>
        /// <returns>true if the supplied path exists in this library, false otherwise</returns>
        public virtual Task<bool> CacheContains(string path) {
            var folders = Folders.Values;
            // folders should be immutable, this is dangerous, but risk accepted
            foreach(var folder in folders) {
                var exists = folder.FirstOrDefault(item => item.Path == path) != null;
                if(exists) {
                    return TaskEx.FromResult(true);
                }
            }
            return TaskEx.FromResult(Folders.ContainsKey(path));
        }
        public MusicItem SearchItem(string path) {
            foreach(var items in Folders.Values) {
                var searchResult = items.FirstOrDefault(i => i.Path == path);
                if(searchResult != null) {
                    return searchResult;
                }
            }
            return null;
        }
        public virtual void Reset() {
            Folders.Clear();
        }
        public bool IsFolderLoaded(string folderId) {
            return Folders.ContainsKey(folderId);
        }
        public virtual async Task LoadFolder(string folderId, ObservableCollection<MusicItem> to) {
            if(IsFolderLoaded(folderId)) {
                AddDistinct(Folders[folderId], to);
            } else {
                await LoadFolderAsync2(folderId, to);
                // saves to cache
                Folders[folderId] = to.OrderBy(MusicItemFolder.SortKey);
            }
        }
        /// <summary>
        /// Assumes 'from' contains no duplicates.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        protected void AddDistinct(IEnumerable<MusicItem> from, ObservableCollection<MusicItem> to) {
            var comparer = new MusicItemComparer();
            // 'newItems' contains the 'from' items which are not already in 'to'.
            // This is algorithmically expensive. Should be fine with <1000 items though.
            var newItems = from.Where(item => !to.Any(toItem => comparer.Equals(item, toItem))).ToList();
            foreach(var newItem in newItems) {
                to.Add(newItem);
            }
            OnNewItemsLoaded(newItems);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns>the songs under the given folder, recursively</returns>
        public async Task<List<MusicItem>> SongsInFolder(MusicItem folder) {
            var items = await ItemsIn(folder);
            var folders = items.Where(item => item.IsDir).ToList();
            var ret = new List<MusicItem>();
            foreach(var dir in folders) {
                var dirSongs = await SongsInFolder(dir);
                ret.AddRange(dirSongs);
            }
            foreach(var song in items) {
                if(!song.IsDir) {
                    ret.Add(song);
                }
            }
            return ret;
        }
        private async Task<IEnumerable<MusicItem>> ItemsIn(MusicItem folder) {
            var folderId = DirectoryIdentifier(folder);
            return await LoadFolderAsync(folderId);
        }
        protected void OnNewItemsLoaded(IEnumerable<MusicItem> items) {
            if(NewItemsLoaded != null) {
                NewItemsLoaded(items);
            }
        }
    }
}
