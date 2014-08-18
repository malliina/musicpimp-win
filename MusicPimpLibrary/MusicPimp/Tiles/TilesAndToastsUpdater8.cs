using Mle.MusicPimp.ViewModels;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Tiles {
    public class TilesAndToastsUpdater8 : Tiles8 {
        private static TilesAndToastsUpdater8 instance = null;
        public static TilesAndToastsUpdater8 Current {
            get {
                if(instance == null)
                    instance = new TilesAndToastsUpdater8();
                return instance;
            }
        }
        public override async Task Update(MusicItem track) {
            await Toasts.Instance.Update(track);
            await base.Update(track);
        }
    }
}
