using Mle.IO;
using Mle.Network;

namespace Mle.MusicPimp.Tiles {
    public class PhoneCoverService : CoverService {
        public static readonly string CoverFolder = "shared/shellcontent/CoverArt/";

        private static PhoneCoverService instance = null;
        public static PhoneCoverService Instance {
            get {
                if(instance == null)
                    instance = new PhoneCoverService();
                return instance;
            }
        }

        public PhoneCoverService()
            : base(PhoneFileUtils.Instance, CoverFolder) {
            // TODO create cover folder
        }
    }
}
