using Mle.Tiles;
using Windows.ApplicationModel.Background;

namespace StoreBackgroundTask {
    public sealed class BackgroundTask : IBackgroundTask {

        public async void Run(IBackgroundTaskInstance taskInstance) {
            var deferral = taskInstance.GetDeferral();
            await PimpTiles.UpdateLiveTiles(TileUtil8.Instance);
            deferral.Complete();
        }
    }
}
