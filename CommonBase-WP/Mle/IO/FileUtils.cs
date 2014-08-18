using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;

namespace Mle.IO {
    /// <summary>
    /// helper methods that operate on isolated storage
    /// </summary>
    public class FileUtils {
        public static void Delete(string isoFilePath) {
            WithStorage(s => s.DeleteFile(isoFilePath));
        }
        /// <summary>
        /// creates the specified directory in isolated storage if it doesn't already exist
        /// </summary>
        /// <param name="path"></param>
        /// <returns>true if the directory was created, false otherwise</returns>
        public static bool CreateDirIfNotExists(string path) {
            return WithStorage(storage => {
                if (!storage.DirectoryExists(path)) {
                    storage.CreateDirectory(path.TrimEnd('/').TrimEnd('\\'));
                    return true;
                }
                return false;
            });
        }

        public static bool DirectoryExists(string dirPath) {
            return PathTest(s => s.DirectoryExists(dirPath));
        }
        public static bool FileExists(string filePath) {
            return PathTest(s => s.FileExists(filePath));
        }
        public static bool PathTest(Func<IsolatedStorageFile, bool> predicate) {
            return WithStorage<bool>(storage => { return predicate(storage); });
        }
        public static T WithStorage<T>(Func<IsolatedStorageFile, T> op) {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication()) {
                return op(storage);
            }
        }
        public static void WithStorage(Action<IsolatedStorageFile> op) {
            WithStorage<int>(s => {
                op(s);
                return 42;
            });
        }
        public static async Task WithStorageAsync(Func<IsolatedStorageFile, Task> op) {
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication()) {
                await op(storage);
            }
        }
        public static T WithFile<T>(string path,
            Func<IsolatedStorageFileStream, T> op,
            FileAccess access = FileAccess.ReadWrite,
            FileShare share = FileShare.Read) {
            return WithStorage<T>(s => {
                if (!s.FileExists(path)) {
                    throw new FileNotFoundException(path);
                }
                using (var stream = s.OpenFile(path, FileMode.Open, access, share)) {
                    return op(stream);
                }
            });
        }
        public static void WithFile(string path,
            Action<IsolatedStorageFileStream> op,
            FileAccess access = FileAccess.ReadWrite,
            FileShare share = FileShare.Read) {
            WithFile<int>(path, s => { op(s); return 42; }, access, share);
        }
        public static async Task WithFileAsync(string path,
            Func<Stream, Task> op,
            FileAccess access = FileAccess.ReadWrite,
            FileShare share = FileShare.Read) {
            await WithStorageAsync(async s => {
                if (s.FileExists(path)) {
                    using (var stream = s.OpenFile(path, FileMode.Open, access, share)) {
                        await op(stream);
                    }
                } else {
                    using (var stream = s.CreateFile(path)) {
                        await op(stream);
                    }
                }
            });
        }
        public static async Task<T> WithFileAsync<T>(string path,
            Func<Stream, Task<T>> op,
            FileAccess access = FileAccess.ReadWrite,
            FileShare share = FileShare.Read) {
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication()) {
                var exists = storage.FileExists(path);
                using (var stream = storage.OpenFile(path, FileMode.Open, access, share)) {
                    return await op(stream);
                }
            }
        }
        public static T WithFileAsync2<T>(string path,
            Func<Stream, T> op,
            FileAccess access = FileAccess.ReadWrite,
            FileShare share = FileShare.Read) {
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication()) {
                using (var stream = storage.OpenFile(path, FileMode.Open, access, share)) {
                    return op(stream);
                }
            }
        }
        public static T WithFileRead<T>(string path, Func<IsolatedStorageFileStream, T> op) {
            return WithFile<T>(path, op, FileAccess.Read);
        }
        public static void WithFileRead(string path, Action<IsolatedStorageFileStream> op) {
            WithFileRead<int>(path, s => { op(s); return 42; });
        }
        public static Task WithFileReadAsync(string path, Func<Stream, Task> op) {
            return WithFileAsync(path, op, FileAccess.Read);
        }
        public static Task<T> WithFileReadAsyncT<T>(string path,Func<Stream,Task<T>> op){
            return WithFileAsync<T>(path, op, FileAccess.Read, FileShare.Read);
        }
        public static T WithFileReadAsyncG<T>(string path, Func<Stream, T> op) {
            return WithFileAsync2(path, op, FileAccess.Read);
        }
    }
}
