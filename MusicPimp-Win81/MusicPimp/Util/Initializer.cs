using Mle.MusicPimp.Tiles;

namespace Mle.MusicPimp.Util {
    public class Initializer {
        public static void Init() {
            Singletons.TileManager = TilesAndToastsUpdater81.Current;
            Singletons.BackgroundTasks = new BackgroundTaskManager(
                "BackgroundTask81",
                "StoreBackgroundTask81.BackgroundTask81");
        }
    }
}
