using Mle.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace Mle.IO {
    public class StoreFileUtils : FileUtilsBase {

        public StorageFolder Folder { get; private set; }

        public StoreFileUtils(StorageFolder folder) {
            Folder = folder;
        }
        public override async Task Delete(string relativePath) {
            var file = await Folder.GetFileAsync(WindowsSeparators(relativePath));
            await file.DeleteAsync();
        }
        public async Task<Stream> OpenStreamIfExists(string relativePath) {
            if(await Folder.FileExists(relativePath)) {
                return await Folder.OpenStreamForReadAsync(WindowsSeparators(relativePath));
            } else {
                return null;
            }
        }

        public Task<Stream> OpenStream(string relativePath) {
            return Folder.OpenStreamForReadAsync(WindowsSeparators(relativePath));
        }
        public override async Task WithFileReadAsync(string relativePath, Func<Stream, Task> op) {
            using(var stream = await OpenStream(relativePath)) {
                await op(stream);
            }
        }
        public override async Task WithFileWriteAsync(string path, Func<Stream, Task> f) {
            using(var stream = await Folder.OpenStreamForWriteAsync(path, CreationCollisionOption.ReplaceExisting)) {
                await f(stream);
            }
        }
        public override async Task<T> WithFileReadAsync<T>(string relativePath, Func<Stream, T> op) {
            using(var stream = await OpenStream(relativePath)) {
                return op(stream);
            }
        }
        public override async Task<T> WithFileReadAsync2<T>(string relativePath, Func<Stream, Task<T>> op) {
            using(var stream = await OpenStream(relativePath)) {
                return await op(stream);
            }
        }
        public override async Task<string[]> ListFolderNames(string relativeFolder) {
            var storageFolders = await ListFolders(relativeFolder);
            return storageFolders.Select(f => f.Name).ToArray();
        }
        public override async Task<IList<Uri>> ListFilesAsUris(string folderPath) {
            var files = await ListFiles(folderPath);
            List<StorageFile> positiveSizeFiles = new List<StorageFile>();
            foreach(var file in files) {
                if((await file.Size()) > 0) {
                    positiveSizeFiles.Add(file);
                }
            }
            return positiveSizeFiles.Select(UriFor).ToList();
        }

        public Task<IEnumerable<StorageFolder>> ListFolders(string relativeFolder, bool recursive = false) {
            return Folder.ListFolders(relativeFolder, recursive);
        }

        public Task<IEnumerable<StorageFile>> ListFiles(string relativeFolder, bool recursive = false) {
            return Folder.ListFiles(relativeFolder, recursive);
        }
        public override Task<string[]> ListFileNames(string folderPath) {
            return GetPaths<StorageFile>(
                folderPath,
                async folder => await folder.GetFilesAsync(CommonFileQuery.DefaultQuery));
        }
        public async Task<IEnumerable<T>> MapFiles<T>(string folderPath, Func<StorageFile, T> mapper) {
            return await GetPaths<StorageFile, T>(folderPath, async folder => await folder.GetFilesAsync(CommonFileQuery.DefaultQuery), mapper);
        }
        private async Task<string[]> GetPaths<T>(string folderPath, Func<StorageFolder, Task<IReadOnlyList<T>>> pathFunc) where T : IStorageItem {
            var paths = await GetPaths<T, string>(folderPath, pathFunc, f => f.Name);
            return paths.ToArray();
        }
        // this is nonsense
        private async Task<IEnumerable<U>> GetPaths<T, U>(string folderPath, Func<StorageFolder, Task<IReadOnlyList<T>>> pathFunc, Func<T, U> mapper) where T : IStorageItem {
            var paths = await GetStorageItems<T>(folderPath, pathFunc);
            return paths.Select(mapper);
        }
        private async Task<StorageFolder> GetFolder(string folderPath) {
            folderPath = WindowsSeparators(folderPath);
            StorageFolder folder = null;
            if(folderPath == String.Empty) {
                folder = Folder;
            } else {
                folder = await Folder.GetFolderAsync(folderPath);
            }
            return folder;
        }
        public async Task<IEnumerable<T>> GetStorageItems<T>(string folderPath, Func<StorageFolder, Task<IReadOnlyList<T>>> pathFunc) where T : IStorageItem {
            var folder = await GetFolder(folderPath);
            return await pathFunc(folder);
        }
        public override async Task<ulong> SizeOfDirectory(string folder) {
            var storageFolder = folder == String.Empty ? Folder : await Folder.GetFolderAsync(WindowsSeparators(folder));
            var files = await storageFolder.GetFilesAsync(CommonFileQuery.OrderByName);
            var folders = await storageFolder.GetFoldersAsync();
            IEnumerable<Task<ulong>> fileSizeTasks = files.Select(async file => {
                var props = await file.GetBasicPropertiesAsync();
                return props.Size;
            });
            ulong total = 0;
            foreach(var task in fileSizeTasks) {
                total += await task;
            }
            return total;
        }
        public override async Task<UriInfo> UriInfoIfExists(string relativePath) {
            try {
                var file = await Folder.GetFileAsync(WindowsSeparators(relativePath));
                return new UriInfo(UriFor(file), await file.Size());
            } catch(FileNotFoundException) {
                return null;
            }
        }
        public async Task<UriInfo> UriInfoIfExists2(string relativePath) {
            var file = await Folder.GetFileIfExists(WindowsSeparators(relativePath));
            if(file != null) {
                return new UriInfo(UriFor(file), await file.Size());
            } else {
                return null;
            }
        }

        /// <summary>
        /// Must be overridden for special app folders, todo fix class hierarchy.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public virtual Uri UriFor(IStorageFile file) {
            var uriPath = FileUtilsBase.UnixSeparators(file.Path);
            return new Uri("file:///" + uriPath);
        }
        /// <summary>
        /// The local file referenced by the URI, or null if the URI is not local.
        /// </summary>
        /// <param name="localUri">uri to local file</param>
        /// <returns></returns>
        public static async Task<StorageFile> GetFile(Uri localUri) {
            if(FileUtilsBase.IsLocalNonAppFile(localUri)) {
                return await StorageFile.GetFileFromPathAsync(localUri.LocalPath);
            } else if(FileUtilsBase.IsAppLocalFile(localUri)) {
                return await StorageFile.GetFileFromApplicationUriAsync(localUri);
            } else {
                return null;
            }
        }


    }
}
