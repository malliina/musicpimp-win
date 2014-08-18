using Mle.Util;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Mle.IO {
    public class AppLocalFolderFileUtils : StoreFileUtils {
        private string localStorageUriPrefix = "ms-appdata:///local/";

        public static string LocalPath(IStorageItem appLocalFile) {
            var keyword = "\\LocalState\\";
            var absolutePath = appLocalFile.Path;
            var startIdx = absolutePath.IndexOf(keyword) + keyword.Length;
            return absolutePath.Substring(startIdx, absolutePath.Length - startIdx);
        }

        public AppLocalFolderFileUtils(StorageFolder appLocalFolder)
            : base(appLocalFolder) {
        }
        public AppLocalFolderFileUtils() : this(ApplicationData.Current.LocalFolder) { }

        public override async Task<UriInfo> UriInfoIfExists(string relativePath) {
            var fileOpt = await Folder.GetFileIfExists(relativePath);
            if (fileOpt != null) {
                var uri = UriFor(fileOpt);
                var props = await fileOpt.GetBasicPropertiesAsync();
                return new UriInfo(uri, props.Size);
            } else {
                return null;
            }
        }
        public async Task<UriInfo> UriInfo2(string relativePath) {
            var file = await Folder.GetFileIfExists(WindowsSeparators(relativePath));
            if (file != null) {
                return new UriInfo(new Uri(file.Path), await file.Size());
            } else {
                return null;
            }
        }
        public override Uri UriFor(IStorageFile file) {
            var path = localStorageUriPrefix + FileUtilsBase.UnixSeparators(LocalPath(file));
            return new Uri(path);
        }
    }
}
