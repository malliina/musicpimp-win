using Mle.Concurrent;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Audio {
    /// <summary>
    /// TODO DRY; code similar to MultiFolderLibraryBase.
    /// </summary>
    public abstract class MultiLibrary : MusicLibrary {
        public abstract ObservableCollection<MusicLibrary> Libraries { get; protected set; }

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
            await lib.Upload(song, resource, destSession);
        }
        protected override async Task<IEnumerable<MusicItem>> LoadFolderAsync(string id) {
            var to = new ObservableCollection<MusicItem>();
            await LoadFolderAsync2(id, to);
            return to.ToList();
        }
        public override async Task LoadFolderAsync2(string id, ObservableCollection<MusicItem> to) {
            foreach(var lib in Libraries) {
                try {
                    await lib.LoadFolderAsync2(id, to);
                } catch(Exception) {
                    // May throw if the sublibrary does not contain a folder with
                    // the given ID. What do we want to do? For now, we suppress
                    // and don't merge anything to the aggregate music items.
                }
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
