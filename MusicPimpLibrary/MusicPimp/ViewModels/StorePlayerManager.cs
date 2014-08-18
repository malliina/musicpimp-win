using Mle.MusicPimp.Pimp;
using Mle.Roaming.Network;
using System;

namespace Mle.MusicPimp.ViewModels {

    public class StorePlayerManager : PlayerManager {

        private static StorePlayerManager instance = null;
        public static StorePlayerManager Instance {
            get {
                if(instance == null)
                    instance = new StorePlayerManager();
                return instance;
            }
        }
        protected StorePlayerManager()
            : base(new RoamingSettings()) {
        }
        public Func<MusicEndpoint, PimpSessionBase> PimpSession() {
            return e => new StorePimpSession(e);
        }
    }
}
