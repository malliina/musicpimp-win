using Mle.Util;
using Mle.ViewModels;
using System;

namespace Mle.MusicPimp.ViewModels {
    public class StartSettings : ViewModelBase {
        private static StartSettings instance = null;
        public static StartSettings Instance {
            get {
                if (instance == null)
                    instance = new StartSettings();
                return instance;
            }
        }

        private static readonly string startPageKey = "startPage";
        public Pages StartPage {
            get {
                Pages def = Pages.Home;
                var enumName = settings.Load<string>(startPageKey, def: Pages.Home.ToString());
                Enum.TryParse<Pages>(enumName, out def);
                return def;
            }
            set {
                settings.Save<string>(startPageKey, value.ToString());
                OnPropertyChanged("StartPage");
            }
        }
        private ISettingsManager settings = Settings.Instance;

        protected StartSettings() {
        }
    }
    public enum Pages {
        Home, Library, Player
    }
}
