using Mle.MusicPimp.Local;
using Mle.Util;
using Mle.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.System.UserProfile;

namespace Mle.MusicPimp.ViewModels {
    public class SettingsOverview : ViewModelBase {
        private static SettingsOverview instance = null;
        public static SettingsOverview Instance {
            get {
                if(instance == null) {
                    instance = new SettingsOverview();
                }
                return instance;
            }
        }
        public LibraryManager AudioSources {
            get { return PhoneLibraryManager.Instance; }
        }
        public PlayerManager PlaybackDevices {
            get { return PhonePlayerManager.Instance; }
        }

        public LimitsViewModel Limits { get; private set; }
        public bool IsLockScreenProvider { get { return LockScreenManager.IsProvidedByCurrentApplication; } }
        public string LockScreenSummary {
            get {
                if(IsLockScreenProvider) return "provided by this app";
                else return "not provided by this app";
            }
        }

        public SettingsOverview() {
            Limits = new LimitsViewModel(PhoneLocalLibrary.Instance, Settings.Instance);
        }
        public void UpdateLockScreen() {
            OnPropertyChanged("IsLockScreenProvider");
            OnPropertyChanged("LockScreenSummary");
        }
    }
}
