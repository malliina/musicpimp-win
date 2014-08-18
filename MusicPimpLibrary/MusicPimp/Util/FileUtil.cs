using Mle.IO;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Mle.MusicPimp.Util {
    public static class FileUtil {
        public static async Task<bool> FileExists(this StorageFolder folder, string path) {
            return await GetFileIfExists(folder, path) != null;
        }
        public static async Task<StorageFile> GetFileIfExists(this StorageFolder folder, string path) {
            if (await folder.FileExists(path)) {
                return await folder.GetFileAsync(path);
            } else {
                return null;
            }
        }
        public static async Task<StorageFile> GetFile(MusicItem track) {
            var localUri = await MultiFolderLibrary.Instance.LocalUriIfExists(track);
            if (localUri == null) {
                return null;
            }
            return await StoreFileUtils.GetFile(localUri);
        }
    }
}
