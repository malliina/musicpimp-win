using Mle.MusicPimp.Tiles;

namespace Mle.MusicPimp.Util {
    public class Initializer {
        public static void Init() {
            Singletons.TileManager = TilesAndToastsUpdater8.Current;
            Singletons.BackgroundTasks = new BackgroundTaskManager(
                "BackgroundTask",
                "StoreBackgroundTask.BackgroundTask");
        }
    }
}
