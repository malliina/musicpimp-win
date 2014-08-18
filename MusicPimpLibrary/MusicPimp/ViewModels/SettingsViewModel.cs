using Mle.MusicPimp.Local;
using Mle.Util;

namespace Mle.MusicPimp.ViewModels {
    public class SettingsViewModel {
        private static SettingsViewModel instance = null;
        public static SettingsViewModel Instance {
            get {
                if (instance == null) {
                    instance = new SettingsViewModel();
                }
                return instance;
            }
        }
        public StoreEndpoints Endpoints {
            get { return StoreEndpoints.Instance; }
        }
        public StorePlayerManager PlayerManager {
            get { return StorePlayerManager.Instance; }
        }
        public StoreLibraryManager LibraryManager {
            get { return StoreLibraryManager.Instance; }
        }
        public StartSettings Start {
            get { return StartSettings.Instance; }
        }

        public LimitsViewModel Limits { get; private set; }

        public MultiFolderLibrary LocalLibrary {
            get { return MultiFolderLibrary.Instance; }
        }
        
        protected SettingsViewModel() {
            Limits = new LimitsViewModel(AppLocalFolderLibrary.Instance, Settings.Instance);
        }
    }
}
