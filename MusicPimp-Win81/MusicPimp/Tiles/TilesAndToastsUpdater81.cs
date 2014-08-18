using Mle.MusicPimp.ViewModels;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Tiles {
    public class TilesAndToastsUpdater81 : Tiles81 {
        private static TilesAndToastsUpdater81 instance = null;
        public static TilesAndToastsUpdater81 Current {
            get {
                if(instance == null)
                    instance = new TilesAndToastsUpdater81();
                return instance;
            }
        }
        public override async Task Update(MusicItem track) {
            await Toasts.Instance.Update(track);
            await base.Update(track);
        }
    }
}
