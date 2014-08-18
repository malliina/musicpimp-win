using Mle.Roaming.Network;

namespace Mle.MusicPimp.ViewModels {
    public class StoreLibraryManager : LibraryManager {
        private static StoreLibraryManager instance = null;
        public static StoreLibraryManager Instance {
            get {
                if (instance == null)
                    instance = new StoreLibraryManager();
                return instance;
            }
        }
        protected StoreLibraryManager()
            : base(new RoamingSettings()) {
        }
    }
}
