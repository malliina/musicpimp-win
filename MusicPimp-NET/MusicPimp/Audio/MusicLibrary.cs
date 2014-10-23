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
        /// Triggers when items from a sub-library (or the whole library) have been loaded. UIs may wish to update themselves at this point.
        /// </summary>
        public event Action<IEnumerable<MusicItem>> NewItemsLoaded;
        public Dictionary<string, IEnumerable<MusicItem>> Folders { get; private set; }
        public string RootFolderKey { get; protected set; }
        public virtual string RootEmptyMessage { get; protected set; }
        public abstract Task Ping();
        /// <summary>
        /// Loads the specified folder. Implementation: No duplicates, no caching, no sorting.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract Task<IEnumerable<MusicItem>> Reload(string id);
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
        /// <summary>
        /// LoadFolder(id,to) -> LoadAndMerge(id,to) -> GetOrLoadAndSet(id) -> Reload(id)
        /// 
        /// MultiLibraries should override LoadAndMerge and call GetOrLoadAndSet(id) one library at a time.
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public async Task LoadFolder(string folderId, ObservableCollection<MusicItem> to) {
            if(IsFolderLoaded(folderId)) {
                AddDistinct(Folders[folderId], to);
                //to.OrderBy(MusicItemFolder.DirOnlySortKey);
            } else {
                await LoadAndMerge(folderId, to);
                // saves to cache
                Folders[folderId] = to;
            }
        }
        /// <summary>
        /// Non-recursively loads the user-selected folder or the root folder if no folder is selected.
        /// 
        /// Mutates 'to' with new items as they are loaded. Uses mutation so that 'to' can be displayed to the user early and
        /// be populated as results arrive.
        /// 
        /// Ensures that to only contains distinct, sorted items.
        /// 
        /// Does not check whether the returned tracks are locally available, so LocalUri == null for all.
        /// 
        /// TODO Document what must happen if a folder with that ID does not exist? Throw exception?
        /// </summary>
        /// <param name="id">the folder identifier; the format is implementation-dependent</param>
        /// <param name="to">the place to put loaded music items</param>
        /// <returns>subfolders and tracks in the given folder</returns>
        public virtual async Task LoadAndMerge(string folderId, ObservableCollection<MusicItem> to) {
            var items = await GetOrLoadAndSet(folderId);
            AddDistinctAndSort(items, to);
        }
        /// <summary>
        /// Gets the ordered contents of the specified folder. Uses the cache, loading and storing on a cache miss. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MusicItem>> GetOrLoadAndSet(string id) {
            if(IsFolderLoaded(id)) {
                return Folders[id];
            } else {
                var items = await Reload(id);
                var ordered = items.OrderBy(MusicItemFolder.DirOnlySortKey).ToList();
                Folders[id] = ordered;
                return ordered;
            }
        }
        public int AddDistinctAndSort(IEnumerable<MusicItem> from, Collection<MusicItem> to) {
            var newItems = from.Count();
            if(newItems > 0) {
                AddDistinct(from, to);
                to.OrderBy(MusicItemFolder.DirOnlySortKey).ToList();
            }
            return newItems;
        }
        public static void Merge(IEnumerable<MusicItem> from, Collection<MusicItem> to) {
            // Good enough approximation, ignores locality checks
            var victims = to.Where(item => from.Any(f => f.Name == item.Name)).ToList();
            foreach(var victim in victims) {
                to.Remove(victim);
            }
            foreach(var item in from) {
                to.Add(item);
            }
        }
        /// <summary>
        /// Adds from to to. Replaces any items in to with the item from from, if two items have the same name.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        protected void AddDistinct(IEnumerable<MusicItem> from, Collection<MusicItem> to) {
            Merge(from, to);
            OnNewItemsLoaded(from);
        }
        public virtual Task<List<SavedPlaylistMeta>> LoadPlaylists() {
            return EmptyListTask<SavedPlaylistMeta>();
        }
        public virtual Task<List<MusicItem>> LoadPlaylist(string playlistID) {
            return EmptyListTask<MusicItem>();
        }
        public virtual Task DeletePlaylist(string playlistID) {
            return AsyncTasks.Noop();
        }
        private Task<List<T>> EmptyListTask<T>() {
            return TaskEx.FromResult<List<T>>(new List<T>());
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
            return await GetOrLoadAndSet(folderId);
        }
        protected void OnNewItemsLoaded(IEnumerable<MusicItem> items) {
            if(NewItemsLoaded != null) {
                NewItemsLoaded(items);
            }
        }
    }
}
