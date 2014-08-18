using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace Mle.IO {
    public static class StorageExtensions {

        private static PathHelper Paths {
            get { return PathHelper.Instance; }
        }

        public static async Task<bool> IsEmpty(this IStorageFolder folder) {
            var files = await folder.GetFilesAsync();
            var folders = await folder.GetFoldersAsync();
            return files.Count + folders.Count == 0;
        }
        /// <summary>
        /// Lists subfolders. If a recursive search is performed, 
        /// the returned list contains the folders ordered from
        /// shallow to deep.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="recursive"></param>
        /// <returns>the subfolders</returns>
        public static async Task<IEnumerable<StorageFolder>> ListFolders(this IStorageFolder folder, bool recursive = false) {
            var shallowSubfolders = await folder.GetFoldersAsync();
            var ret = new List<StorageFolder>(shallowSubfolders);
            if(recursive) {
                foreach(var subFolder in shallowSubfolders) {
                    var subFolders = await ListFolders(subFolder, recursive: true);
                    ret.AddRange(subFolders);
                }
            }
            return ret;
        }
        /// <summary>
        /// Throws if the relative folder does not exist.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="relativeFolder"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static Task<IEnumerable<StorageFolder>> ListFolders(this StorageFolder folder, string relativeFolder, bool recursive = false) {
            return folder.WithFolder<StorageFolder>(relativeFolder, f => f.ListFolders(recursive));
        }
        public static async Task<IEnumerable<StorageFile>> ListFiles(this StorageFolder folder, bool recursive = false) {
            if(recursive) {
                return await folder.GetFilesAsync(CommonFileQuery.OrderByName);
            } else {
                return await folder.GetFilesAsync();
            }
        }
        /// <summary>
        /// Throws if the relative folder does not exist.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="relativeFolder"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static Task<IEnumerable<StorageFile>> ListFiles(this StorageFolder folder, string relativeFolder, bool recursive = false) {
            return folder.WithFolder(relativeFolder, f => f.ListFiles(recursive));
        }
        private static async Task<IEnumerable<T>> WithFolder<T>(this StorageFolder folder, string relativeFolder, Func<StorageFolder, Task<IEnumerable<T>>> op) where T : IStorageItem {
            StorageFolder f = relativeFolder == String.Empty ?
                folder :
                await folder.GetFolderAsync(FileUtilsBase.WindowsSeparators(relativeFolder));
            return await op(f);
        }
        public static async Task<bool> FolderExists(this StorageFolder folder, string folderPath) {
            if(String.IsNullOrEmpty(folderPath)) {
                return true;
            }
            var firstPart = Paths.RootFolderName(folderPath);
            if(await folder.ContainsFolderAsync(firstPart)) {
                var subFolder = await folder.GetFolderAsync(firstPart);
                var tail = Paths.DropRoot(folderPath, firstPart);
                return await subFolder.FolderExists(tail);
            } else {
                return false;
            }
        }
        public static async Task<bool> FileExists(this StorageFolder folder, string filePath) {
            return await folder.InspectFilePath<bool>(
                filePath,
                files => files.Any);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="filePath"></param>
        /// <returns>the file at the given path relative to this folder, or null if no file exists at the path</returns>
        public static async Task<StorageFile> GetFileIfExists(this StorageFolder folder, string filePath) {
            return await folder.InspectFilePath<StorageFile>(
                filePath,
                files => files.FirstOrDefault);
        }
        /// <summary>
        /// FP in C#.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="folder"></param>
        /// <param name="filePath"></param>
        /// <param name="filesCombinator"></param>
        /// <param name="notFound"></param>
        /// <returns></returns>
        private static async Task<T> InspectFilePath<T>(this StorageFolder folder, string filePath, Func<IReadOnlyList<StorageFile>, Func<Func<StorageFile, bool>, T>> filesCombinator, T notFound = default(T)) {
            var fileName = Paths.FileNameOf(filePath);
            var firstPart = Paths.RootFolderName(filePath);
            if(firstPart == fileName) {
                var files = (await folder.GetFilesAsync());
                return filesCombinator(files)(file => file.Name == fileName);
            }
            if(await folder.ContainsFolderAsync(firstPart)) {
                var subFolder = await folder.GetFolderAsync(firstPart);
                var tail = Paths.DropRoot(filePath, firstPart);
                return await subFolder.InspectFilePath(tail, filesCombinator, notFound);
            } else {
                return notFound;
            }
        }
        public static async Task<ulong> Size(this StorageFile file) {
            var props = await file.GetBasicPropertiesAsync();
            return props.Size;
        }
        /// <summary>
        /// Only searches immediate subfolders; see FolderExists to search a path.
        /// 
        /// taken from http://winrtxamltoolkit.codeplex.com/SourceControl/latest#WinRTXamlToolkit/IO/Extensions/StorageFolderExtensions.cs
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async Task<bool> ContainsFolderAsync(this StorageFolder folder, string name) {
            return (await folder.GetFoldersAsync()).Any(l => l.Name == name);
        }
    }
}
