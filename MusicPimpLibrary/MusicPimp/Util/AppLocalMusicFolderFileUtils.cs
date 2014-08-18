using Mle.IO;
using Mle.MusicPimp.Local;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Mle.MusicPimp.Util {
    public class AppLocalMusicFolderFileUtils {
        private static AppLocalFolderFileUtils instance = null;
        public static AppLocalFolderFileUtils Instance {
            get { return instance; }
        }
        public static async Task Init() {
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
                AppLocalFolderLibrary.musicFolderName,
                CreationCollisionOption.OpenIfExists);
            instance = new AppLocalFolderFileUtils(folder);
        }
    }
}
