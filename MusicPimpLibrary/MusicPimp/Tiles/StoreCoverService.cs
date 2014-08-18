using Mle.IO;
using Mle.Tiles;

namespace Mle.MusicPimp.Tiles {
    public class StoreCoverService : CoverService {

        private static StoreCoverService instance = null;
        public static StoreCoverService Instance {
            get {
                if(instance == null)
                    instance = new StoreCoverService();
                return instance;
            }
        }

        protected StoreCoverService()
            : base(new AppLocalFolderFileUtils(), PimpTiles.CoverArtFolderPath) {
        }
    }
}
