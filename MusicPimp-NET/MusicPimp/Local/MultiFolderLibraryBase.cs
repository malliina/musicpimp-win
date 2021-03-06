﻿using Mle.MusicPimp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Local {
    public abstract class MultiFolderLibraryBase : LocalMusicLibrary {

        public ObservableCollection<LocalLibraryBase> Libraries { get; private set; }
        public IEnumerable<LocalLibraryBase> UserAddedLibraries {
            get { return Libraries.Count > 0 ? Libraries.Skip(1).ToList() : new List<LocalLibraryBase>(); }
        }
        public MultiFolderLibraryBase(ObservableCollection<LocalLibraryBase> libraries) {
            Libraries = libraries;
            Libraries.CollectionChanged += (s, e) => {
                OnPropertyChanged("UserAddedLibraries");
            };
        }
        public MultiFolderLibraryBase(LocalLibraryBase initialLibrary)
            : this(new ObservableCollection<LocalLibraryBase>() { initialLibrary }) {
        }
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
        public override async Task LoadAndMerge(string id, ObservableCollection<MusicItem> to) {
            var loaded = new List<MusicItem>();
            foreach(var lib in Libraries) {
                try {
                    var items = await lib.LoadFolderIfExists(id);
                    if(AddDistinctNoSort(items, to) > 0) {
                        loaded.AddRange(items);
                    }
                } catch(Exception) {
                    // May throw if the sublibrary does not contain a folder with
                    // the given ID. What do we want to do? For now, we suppress
                    // and don't merge anything to the aggregate music items.
                }
            }
            Folders[id] = loaded.OrderBy(MusicItemFolder.DirOnlySortKey).ToList();
        }
        public override async Task<T> WithStream<T>(MusicItem track, Func<Stream, Task<T>> op) {
            foreach(var lib in Libraries) {
                if(await lib.ContainsFile(track.Path)) {
                    return await lib.FileUtil.WithFileReadAsync2(track.Path, op);
                }
            }
            throw new FileNotFoundException(track.Path);
        }
        public override async Task<Uri> LocalUriIfExists(MusicItem track) {
            if(track.IsSourceLocal) {
                return track.Source;
            }
            return await Choose(track, async lib => await lib.LocalUriIfExists(track));
        }
        //public override Task<Stream> OpenStreamIfExists(MusicItem track) {
        //    return Choose(track, async lib => await lib.OpenStreamIfExists(track));
        //}
        private async Task<T> Choose<T>(MusicItem track, Func<LocalLibraryBase, Task<T>> op) {
            foreach(var lib in Libraries) {
                var maybeResult = await op(lib);
                if(maybeResult != null) {
                    return maybeResult;
                }
            }
            return default(T);
        }
        public override async Task Delete(List<string> songs) {
            var songsGroupedByLibrary = songs.GroupBy(FindLibrary).ToList();
            foreach(var songGroup in songsGroupedByLibrary) {
                var key = await songGroup.Key;
                await key.Delete(songGroup.ToList());
            }
        }
        public async Task<LocalLibraryBase> FindLibrary(string relativePath) {
            foreach(var lib in Libraries) {
                if(await lib.CacheContains(relativePath)) {
                    return lib;
                }
            }
            // well, no. fix this or rename the method.
            throw new FileNotFoundException(relativePath);
        }
        // sorts directories first, then based on name
        private string SortKey(MusicItem item) {
            string prefix = item.IsDir ? "a" : "b";
            return prefix + item.Name;
        }
    }
}
