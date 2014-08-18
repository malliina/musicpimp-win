using Mle.Concurrent;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.IO {
    public class PhoneFileUtils : FileUtilsBase {
        private static PhoneFileUtils instance = null;
        public static PhoneFileUtils Instance {
            get {
                if(instance == null)
                    instance = new PhoneFileUtils();
                return instance;
            }
        }
        public override Task Delete(string relativePath) {
            FileUtils.WithStorage(s => s.DeleteFile(relativePath));
            return AsyncTasks.Noop();
        }
        public override Task WithFileReadAsync(string path, Func<Stream, Task> op) {
            return FileUtils.WithFileReadAsync(path, op);
        }
        public override Task WithFileWriteAsync(string path, Func<Stream, Task> f) {
            return FileUtils.WithFileAsync(path, f, FileAccess.ReadWrite, FileShare.Read);
        }
        public override Task<T> WithFileReadAsync<T>(string path, Func<Stream, T> op) {
            return TaskEx.FromResult(FileUtils.WithFileReadAsyncG<T>(path, op));
        }
        public override Task<T> WithFileReadAsync2<T>(string path, Func<Stream, Task<T>> op) {
            return FileUtils.WithFileReadAsyncT(path, op);
        }
        public override Task<string[]> ListFileNames(string path) {
            return TaskEx.Run(() => {
                return FileUtils.WithStorage(storage => {
                    return storage.GetFileNames(EnsureEndsWithSlashStar(path));
                });
            });
        }
        public override Task<string[]> ListFolderNames(string path) {
            return TaskEx.Run(() => {
                return FileUtils.WithStorage(storage => {
                    return storage.GetDirectoryNames(EnsureEndsWithSlashStar(path));
                });
            });
        }
        public override Task<IList<Uri>> ListFilesAsUris(string path) {
            return TaskEx.Run(() => {
                var files = ListFilePaths(path);
                //var files = await ListFileNames(path);
                IList<Uri> uris = files.Select(filePath => new Uri(filePath, UriKind.Relative)).ToList();
                return uris;
            });
        }
        public IEnumerable<string> ListFolderPaths(string folderPath, bool recursive = false) {
            return ListStorageItemPaths(s => s.GetDirectoryNames, folderPath, recursive);
        }
        public IEnumerable<string> ListFilePaths(string folderPath, bool recursive = false) {
            return FileUtils.WithStorage(s => {
                return ListFilePaths(s, folderPath, recursive);
            });
        }
        private IEnumerable<string> ListFilePaths(IsolatedStorageFile storage, string path, bool recursive) {
            var folderPath = UnixSeparators(path);
            if(!folderPath.EndsWith("/")) {
                folderPath += "/";
            }
            var folderPattern = folderPath + "*";
            var files = storage.GetFileNames(folderPattern)
                .Select(name => folderPath + name)
                .ToList();
            if(recursive) {
                var subFolders = storage.GetDirectoryNames(folderPattern);
                foreach(var folderName in subFolders) {
                    files.AddRange(ListFilePaths(storage, folderPath + folderName, recursive));
                }
            }
            return files;
        }
        private IEnumerable<string> ListStorageItemPaths(Func<IsolatedStorageFile, Func<string, string[]>> storageFunc, string path, bool recursive = false) {
            return FileUtils.WithStorage(s => {
                var subItemNames = storageFunc(s)(EnsureEndsWithSlashStar(path));
                var paths = subItemNames.Select(name => path + "/" + name).ToList();
                if(recursive) {
                    foreach(var p in paths) {
                        paths.AddRange(ListStorageItemPaths(storageFunc, p, recursive: true));
                    }
                }
                return paths;
            });

        }
        public override Task<UriInfo> UriInfoIfExists(string absolutePath) {
            return TaskEx.FromResult(UriData(absolutePath));
        }
        /// <summary>
        /// Note for WP8: System.IO.PathTooLongException: The specified path, 
        /// file name, or both are too long. The fully qualified file name 
        /// must be less than 260 characters, and the directory name must be 
        /// less than 248 characters.
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public UriInfo UriData(string absolutePath) {
            return FileUtils.WithStorage<UriInfo>(s => {
                if(s.FileExists(absolutePath)) {
                    long size = -1;
                    using(var file = s.OpenFile(absolutePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                        size = file.Length;
                    }
                    return new UriInfo(new Uri(absolutePath, UriKind.Relative), (ulong)size);
                } else {
                    return null;
                }
            });
        }
        public Uri GetUri(string absolutePath, ulong expectedSize) {
            var info = UriData(absolutePath);
            return GetUriIfCorrectSize(info, expectedSize);
        }

        public override Task<ulong> SizeOfDirectory(string folder) {
            return TaskEx.Run(() => {
                using(var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication()) {
                    return SizeOfDirectory(isolatedStorage, folder);
                }
            });

        }
        private ulong SizeOfDirectory(IsolatedStorageFile storage, string directory) {
            var dirPattern = EnsureEndsWithSlashStar(directory);
            ulong total = 0;
            foreach(var dir in storage.GetDirectoryNames(dirPattern)) {
                total += SizeOfDirectory(storage, directory + dir + "/");
            }
            foreach(var fileName in storage.GetFileNames(dirPattern)) {
                try {
                    using(var file = storage.OpenFile(directory + fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                        total += (ulong)file.Length;
                    }
                } catch(IsolatedStorageException) { }
            }
            return total;
        }



    }
}
