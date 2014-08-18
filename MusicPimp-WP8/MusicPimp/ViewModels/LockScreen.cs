using Mle.MusicPimp.Util;
using Mle.ViewModels;
using Mle.Xaml.Commands;
using System;
using System.Windows.Input;
using Windows.Phone.System.UserProfile;
using Windows.System;

namespace Mle.MusicPimp.ViewModels {
    public class LockScreen : ViewModelBase {
        private static LockScreen instance = null;
        public static LockScreen Instance {
            get {
                if(instance == null)
                    instance = new LockScreen();
                return instance;
            }
        }
        public bool IsProvidedByApp {
            get { return LockScreenManager.IsProvidedByCurrentApplication; }
        }
        public string IsProvidedByAppText {
            get { return "MusicPimp is currently " + (IsProvidedByApp ? "" : "NOT ") + "the lock screen background provider."; }
        }
        public ICommand GoToLockScreenSettings { get; private set; }
        public ICommand RequestLockScreenAccess { get; private set; }
        protected LockScreen() {
            GoToLockScreenSettings = new AsyncUnitCommand(async () => await Launcher.LaunchUriAsync(new Uri("ms-settings-lock:")));
            RequestLockScreenAccess = new AsyncUnitCommand(async () => {
                await LockScreenRequest.RequestThenSetLockScreen();
                CheckIsAppProvider();
            });
        }
        public void CheckIsAppProvider() {
            OnPropertyChanged("IsProvidedByApp");
            OnPropertyChanged("IsProvidedByAppText");
            SettingsOverview.Instance.UpdateLockScreen();
        }
    }
}
