using Mle.IO;
using Mle.IO.Local;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Util;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Local {
    public abstract class LocalLibraryBase : LocalMusicLibrary {
        public abstract string BaseMusicPath { get; protected set; }
        public abstract SongInfo ReadId3(string filePath, Stream stream);
        public abstract IPathHelper Paths { get; set; }
        public abstract FileUtilsBase FileUtil { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativeFile"></param>
        /// <returns>true if the library contains the item, false otherwise</returns>
        public abstract Task<bool> ContainsFile(string relativeFile);
        public abstract Task<bool> ContainsFolder(string relativeFolder);
        public ISettingsManager SettingsManager { get; private set; }

        public LocalLibraryBase(ISettingsManager settings) {
            SettingsManager = settings;
        }
        public override Task<T> WithStream<T>(MusicItem track, Func<Stream, Task<T>> op) {
            return FileUtil.WithFileReadAsync2(BaseMusicPath + track.Path, op);
        }
        public override async Task<bool> CacheContains(string path) {
            return (await ContainsFile(path)) || (await ContainsFolder(path));
        }
        /// <summary>
        /// Loads the folder, returning an empty list if the folder does not exist in the library.
        /// 
        /// See also LoadFolderAsync(folder), which throws an exception if the folder does not exist.
        /// </summary>
        /// <param name="folderId">the folder path relative to the root folder</param>
        /// <returns></returns>
        public async Task<IEnumerable<MusicItem>> LoadFolderIfExists(string folderId) {
            if(await ContainsFolder(folderId)) {
                return await LoadFolderAsync(folderId);
            } else {
                return new List<MusicItem>();
            }
        }
        /// <summary>
        /// Reads ID3 tags, if any, of the file at the specified path.
        /// If no tags are found, returns metadata based on the file path.
        /// </summary>
        /// <param name="filePath">path to MP3 in this library</param>
        /// <returns>track metadata for the file at the specified path</returns>
        public async Task<SongInfo> FromId3OrElseFile(string filePath) {
            try {
                return await FileUtil.WithFileReadAsync<SongInfo>(BaseMusicPath + filePath, stream => {
                    return ReadId3(filePath, stream);
                });
            } catch(IndexOutOfRangeException) {
                // maybe thrown if the file is cocked up and id3 tags cannot be read
                return FromFile(filePath);
            }
        }
        public override string DirectoryIdentifier(MusicItem musicDir) {
            return musicDir.Path;
        }
        public Task<UriInfo> UriInfo(string path) {
            var absolutePath = BaseMusicPath + path;
            return FileUtil.UriInfoIfExists(absolutePath);
        }
        public Task<Uri> LocalUriForIfExists(string path, ulong expectedSize) {
            return FileUtil.UriIfExists(BaseMusicPath + path, expectedSize);
        }
        public override async Task<Uri> LocalUriIfExists(MusicItem track) {
            if(track.IsSourceLocal) {
                return track.Source;
            }
            return await LocalUriForIfExists(track.Path, (ulong)track.Size);
        }
        public string AbsolutePathTo(MusicItem track) {
            return BaseMusicPath + track.Path;
        }
        protected override async Task<IEnumerable<MusicItem>> LoadFolderAsync(string folder) {
            var relativeFolder = "";
            if(folder != String.Empty) {
                relativeFolder = folder + "/";
            }
            if(await ContainsFolder(relativeFolder)) {
                return await LoadMusicFiles(BaseMusicPath, relativeFolder);
            } else {
                return new List<MusicItem>();
            }
        }
        private Task<List<MusicItem>> LoadMusicFiles(string root, string folder) {
            return TaskEx.Run<List<MusicItem>>(async () => {
                var items = await GetFolders(root, folder);
                var files = await GetSongs(root, folder);
                items.AddRange(files);
                return items;
            });
        }
        private async Task<List<MusicItem>> GetFolders(string root, string folder) {
            // todo clarify concepts; relative vs absolute
            var relativeFolder = root + folder;
            var ret = new List<MusicItem>();
            var dirs = await FileUtil.ListFolderNames(relativeFolder);
            foreach(var dirName in dirs) {
                var path = FileUtilsBase.UnixSeparators(folder + dirName);
                ret.Add(new MusicItem() {
                    Id = path,
                    Name = dirName,
                    IsDir = true,
                    Path = path
                });
            }
            return ret;
        }
        protected virtual async Task<IEnumerable<MusicItem>> GetSongs(string root, string folder) {
            var ret = new List<MusicItem>();
            var relativeFolder = root + folder;
            var files = await FileUtil.ListFileNames(relativeFolder);
            foreach(var file in files) {
                if(Paths.ExtensionOf(file) == ".mp3") {
                    // parses the artist and album from the file path
                    var virtualPath = folder + file;
                    var uriInfo = await UriInfo(virtualPath);
                    var item = await BuildMusicItem(virtualPath, uriInfo.Uri, (long)uriInfo.Size);
                    ret.Add(item);
                }
            }
            return ret;
        }
        protected virtual async Task<MusicItem> BuildMusicItem(string virtualPath, Uri localUri, long size) {
            var songInfo = await FromId3OrElseFile(virtualPath);
            return new MusicItem() {
                Id = virtualPath,
                Name = songInfo.Title,
                Album = songInfo.Album,
                Artist = songInfo.Artist,
                IsDir = false,
                Path = virtualPath,
                Source = localUri,
                Size = size,
                Duration = songInfo.Duration
            };
        }
        public SongInfo FromFile(string filePath) {
            var dirPath = Paths.DirectoryNameOf(filePath);
            var album = dirPath;
            album = Paths.FileNameOf(album);
            var artist = String.Empty;
            if(dirPath != String.Empty)
                artist = Paths.DirectoryNameOf(dirPath);
            artist = Paths.FileNameOf(artist);
            if(artist == String.Empty) {
                artist = album;
            }
            var title = Paths.FileNameWithoutExtension(filePath);
            return new SongInfo(title, artist, album, TimeSpan.FromSeconds(0));
        }
        public Task<ulong> ConsumedDiskSpaceBytes() {
            return FileUtil.SizeOfDirectory(BaseMusicPath);
        }

        public async Task MaintainCacheLimit() {
            long cacheSizeBytes = (long)(await ConsumedDiskSpaceBytes());
            long limitGb = SettingsManager.Load<int>(SettingKey.maxCacheSizeGb.ToString(), LimitsViewModel.DEFAULT_CACHE_SIZE_GB);
            long cacheLimitBytes = 1024L * limitGb * 1024 * 1024;
            // deletes x songs, where x * averageSongSize == (cache size - cache limit)
            ulong averageSongSize = 5000000;
            // estimates how many songs to delete in order to get back to within cache limit
            if(cacheSizeBytes > cacheLimitBytes) {
                // todo this actually overflowed at least before the above condition was in place, 
                // do the maths properly
                var deletableSongCount = (int)(1.0D * (cacheSizeBytes - cacheLimitBytes) / averageSongSize);
                if(deletableSongCount > 1) {
                    // delete songs
                    await DeleteLeastPlayed(deletableSongCount);
                }
            }
        }
        public abstract Task DeleteLeastPlayed(int count = 20);
    }
}
