using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Audio {
    /// <summary>
    /// TODO DRY; code similar to MultiFolderLibraryBase.
    /// </summary>
    public abstract class MultiLibrary : MusicLibrary {
        public abstract ObservableCollection<MusicLibrary> Libraries { get; protected set; }
        public override async Task<IEnumerable<MusicItem>> Reload(string id) {
            ObservableCollection<MusicItem> items = new ObservableCollection<MusicItem>();
            foreach(var lib in Libraries) {
                var subItems = await lib.Reload(id);
                if(subItems.Count() > 0) {
                    AddDistinct(subItems, items);
                }
            }
            return items;
        }
        /// <summary>
        /// Mutates 'to' with new items as they are loaded. Uses mutation so that 'to' can be displayed to the user early and
        /// be populated as results arrive.
        /// </summary>
        /// <param name="id">folder ID</param>
        /// <param name="to">destination collection</param>
        /// <returns></returns>
        public override async Task LoadAndMerge(string id, ObservableCollection<MusicItem> to) {
            var loaded = new List<MusicItem>();
            foreach(var lib in Libraries) {
                try {
                    var items = await lib.GetOrLoadAndSet(id);
                    if(AddDistinctNoSort(items, to) > 0) {
                        loaded.AddRange(items);
                    }
                } catch(Exception) {
                    // May throw if the sublibrary does not contain a folder with
                    // the given ID. What do we want to do? For now, we suppress
                    // and don't merge anything to the aggregate music items.
                }
            }
            Folders[id] = to.OrderBy(MusicItemFolder.DirOnlySortKey).ToList();
        }
        public async Task<MusicLibrary> FindLibrary(string relativePath) {
            foreach(var lib in Libraries) {
                if(await lib.CacheContains(relativePath)) {
                    return lib;
                }
            }
            // well, no. fix this or rename the method.
            throw new FileNotFoundException(relativePath);
        }
        public override async Task Upload(MusicItem song, string resource, PimpSession destSession) {
            var lib = await FindLibrary(song.Path);
            if(lib != null) {
                await lib.Upload(song, resource, destSession);
            }
        }
        public override async Task<IEnumerable<MusicItem>> Search(string term) {
            List<MusicItem> results = new List<MusicItem>();
            foreach(var lib in Libraries) {
                results.AddRange(await lib.Search(term));
            }
            return results;
        }
        public override void Reset() {
            base.Reset();
            foreach(var lib in Libraries) {
                lib.Reset();
            }
        }
    }
}
