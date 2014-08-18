using Mle.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Mle.IO {
    /// <summary>
    /// WinRT and Windows Phone don't share the same file APIs so
    /// we wrap them to improve code reuse.
    /// </summary>
    public abstract class FileUtilsBase {
        public abstract Task Delete(string relativePath);
        //public abstract Task<Stream> OpenStreamIfExists(string relativePath);
        public abstract Task WithFileReadAsync(string path, Func<Stream, Task> op);
        public abstract Task WithFileWriteAsync(string path, Func<Stream, Task> f);
        public abstract Task<T> WithFileReadAsync<T>(string path, Func<Stream, T> op);
        public abstract Task<T> WithFileReadAsync2<T>(string path, Func<Stream, Task<T>> op);
        public abstract Task<IList<Uri>> ListFilesAsUris(string path);
        /// <summary>
        /// Non-recursive.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public abstract Task<string[]> ListFileNames(string path);
        /// <summary>
        /// Lists folders.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public abstract Task<string[]> ListFolderNames(string path);
        public abstract Task<UriInfo> UriInfoIfExists(string absolutePath);
        public abstract Task<ulong> SizeOfDirectory(string folder);
        /// <summary>
        /// Searches for a specific file with a given size.
        /// </summary>
        /// <param name="path">the path to the isolated storage file</param>
        /// <param name="expectedSize">expected file size in bytes</param>
        /// <returns>the uri to the isolated storage file matching the path and size, or null if no such file was found</returns>
        public async Task<Uri> UriIfExists(string path, ulong expectedSize) {
            var uriInfo = await UriInfoIfExists(path);
            return GetUriIfCorrectSize(uriInfo, expectedSize);
        }
        public async Task<Uri> UriIfExists(string path) {
            var uriInfo = await UriInfoIfExists(path);
            return uriInfo != null ? uriInfo.Uri : null;
        }
        public async Task<Uri> UriIfExistsAndPositiveSize(string path) {
            var uriInfo = await UriInfoIfExists(path);
            return GetUriIfCorrectSize(uriInfo, size => size > 0);
        }
        public Uri GetUriIfCorrectSize(UriInfo info, ulong expectedSize) {
            return GetUriIfCorrectSize(info, size => size == expectedSize);
        }
        public Uri GetUriIfCorrectSize(UriInfo info, Func<ulong, bool> sizePredicate) {
            if (info != null && sizePredicate(info.Size)) {
                return info.Uri;
            } else {
                return null;
            }
        }
        /// <summary>
        /// On WP7, the parameter to GetDirectoryNames must end with "/*"
        /// for the correct behavior, whereas on WP8 this is not needed.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string EnsureEndsWithSlashStar(string path) {
            if (path.EndsWith("/*")) {
                return path;
            } else if (path.EndsWith("/")) {
                return path + "*";
            } else {
                return path + "/*";
            }
        }
        /// <summary>
        /// Determines whether the URI points to a local, non-app storage file.
        /// 
        /// The Scheme of a URI is its first part, that is, the protocol.
        /// 
        /// Files in local storage start with: ms-appdata:///local/ and thus have
        /// a Scheme of ms-appdata
        /// 
        /// The Scheme of remote URIs is http, https, ...
        /// 
        /// The Scheme of non-app local files is file (with URIs like file:///path-here...)
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static bool IsLocalNonAppFile(Uri uri) {
            return uri.Scheme == "file";
        }
        public static bool IsAppLocalFile(Uri uri) {
            return uri.Scheme == "ms-appdata";
        }
        public static string WindowsSeparators(string path) {
            return path.Replace('/', '\\');
        }
        public static string UnixSeparators(string path) {
            return path.Replace('\\', '/');
        }
    }
}
