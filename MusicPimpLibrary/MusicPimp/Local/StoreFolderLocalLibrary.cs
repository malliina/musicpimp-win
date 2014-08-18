using Mle.IO;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace Mle.MusicPimp.Local {
    public class StoreFolderLocalLibrary : StoreLocalLibraryBase {
        private static readonly string libraryPrefix = "library-";

        private static StorageItemAccessList FolderAccess {
            get { return StorageApplicationPermissions.FutureAccessList; }
        }

        public static async Task<IEnumerable<StoreFolderLocalLibrary>> InitLibraries() {
            try {
                return (await GetAccessibleFolders()).Select(f => new StoreFolderLocalLibrary(f));
            } catch(Exception) {
                // never happened on my computer but the app cert just failed, so we try something desperate.
                return new List<StoreFolderLocalLibrary>();
            }
        }
        /// <summary>
        /// Adds the folder to the list of folders this app has access to;
        /// prior to this the user must have selected the provided folder
        /// using a folder picker.
        /// </summary>
        /// <param name="accessFolder">folder this app has permission to use</param>
        public static void Save(StorageFolder accessFolder) {
            FolderAccess.AddOrReplace(Tokenize(accessFolder.Path), accessFolder);
        }
        public static void Remove(StorageFolder accessFolder) {
            FolderAccess.Remove(Tokenize(accessFolder.Path));
        }

        private static async Task<IEnumerable<StorageFolder>> GetAccessibleFolders() {
            var ret = new List<StorageFolder>();
            var accessList = StorageApplicationPermissions.FutureAccessList;
            var folderCount = accessList.Entries.Count;
            for(int i = 0; i < folderCount; ++i) {
                var e = accessList.Entries[i];
                var folder = await GetFolder(e.Token);
                if(folder != null) {
                    ret.Add(folder);
                }
            }
            return ret;
        }
        private static async Task<StorageFolder> GetFolder(string accessToken) {
            var path = GetPath(accessToken);
            if(path != null) {
                return await StorageFolder.GetFolderFromPathAsync(path);
            } else {
                return null;
            }
        }
        private static string GetPath(string token) {
            var plainToken = Strings.decode(token);
            if(plainToken.StartsWith(libraryPrefix)) {
                return plainToken.Substring(libraryPrefix.Length);
            } else {
                return null;
            }
        }
        private static string Tokenize(string path) {
            return Strings.encode(libraryPrefix + path);
        }

        public override string BaseMusicPath { get; protected set; }

        private StoreFileUtils fileUtils;
        public override FileUtilsBase FileUtil { get { return fileUtils; } }
        //public override Task<Stream> OpenStreamIfExists(MusicItem track) {
        //    return FileUtil.OpenStreamIfExists(track.Path);
        //}

        public StoreFolderLocalLibrary(StorageFolder rootFolder)
            : base(rootFolder) {
            BaseMusicPath = "";
            fileUtils = new StoreFileUtils(rootFolder);
        }

        public override StoreFileUtils StoreFileUtil {
            get { return fileUtils; }
        }
    }
}
