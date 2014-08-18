using Mle.Tiles;
using Windows.ApplicationModel.Background;

namespace StoreBackgroundTask81 {
    public sealed class BackgroundTask81 : IBackgroundTask {
        public async void Run(IBackgroundTaskInstance taskInstance) {
            var deferral = taskInstance.GetDeferral();
            await PimpTiles.UpdateLiveTiles(TileUtil81.Instance);
            deferral.Complete();
        }
    }
}
