using Mle.IO;
using Mle.MusicPimp.Util;
using Windows.Storage;

namespace Mle.MusicPimp.Local {
    public class AppLocalFolderLibrary : StoreLocalLibraryBase {
        public static readonly string musicFolderName = "music";

        private static AppLocalFolderLibrary instance = null;
        public static AppLocalFolderLibrary Instance {
            get {
                if(instance == null)
                    instance = new AppLocalFolderLibrary(AppLocalMusicFolderFileUtils.Instance.Folder);
                return instance;
            }
        }
        /// <summary>
        /// Resolves and returns the relative music path of the track the given file 
        /// points to. The path is returned with Unix-style path separators.
        /// </summary>
        /// <param name="file"></param>
        /// <returns>the relative music path of the file with Unix-style path separators</returns>
        public static string MusicPath(IStorageFile file) {
            // music\Iron Maiden\...
            var path = AppLocalFolderFileUtils.LocalPath(file);
            // Iron Maiden/...
            return FileUtilsBase.UnixSeparators(path.Substring(musicFolderName.Length + 1));
        }

        public override string BaseMusicPath { get; protected set; }
        public override StoreFileUtils StoreFileUtil {
            get { return AppLocalMusicFolderFileUtils.Instance; }
        }
        protected AppLocalFolderLibrary(StorageFolder root)
            : base(root) {
            RootEmptyMessage = "The local MusicPimp library on this device is empty. Add a MusicPimp server to obtain music.";
            BaseMusicPath = "";
        }
    }
}
