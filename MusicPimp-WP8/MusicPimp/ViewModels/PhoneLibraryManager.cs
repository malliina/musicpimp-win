using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.Subsonic;
using Mle.Util;

namespace Mle.MusicPimp.ViewModels {
    public class PhoneLibraryManager : LibraryManager {
        private static PhoneLibraryManager instance = null;
        public static PhoneLibraryManager Instance {
            get {
                if(instance == null)
                    instance = new PhoneLibraryManager();
                return instance;
            }
        }
        protected PhoneLibraryManager()
            : base(Settings.Instance) {
        }
    }
}
