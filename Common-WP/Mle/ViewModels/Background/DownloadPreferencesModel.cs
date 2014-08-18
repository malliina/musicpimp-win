using Microsoft.Phone.BackgroundTransfer;
using Mle.Util;

namespace Mle.ViewModels.Background {
    public enum DownloadSettings {
        onlyWifiDownloads, onlyExternalPowerDownloads
    }
    public class DownloadPreferencesModel : ViewModelBase {
        private bool load(DownloadSettings setting) {
            return Settings.Instance.Load<bool>(setting.ToString(), def: false);
        }
        private void save(DownloadSettings setting, bool value) {
            Settings.Instance.Save<bool>(setting.ToString(), value);
        }
        public bool WifiOnly {
            get {
                return load(DownloadSettings.onlyWifiDownloads);
            }
            set {
                save(DownloadSettings.onlyWifiDownloads, value);
                OnPropertyChanged("WifiOnly");
            }
        }
        public bool ExternalPowerOnly {
            get {
                return load(DownloadSettings.onlyExternalPowerDownloads);
            }
            set {
                save(DownloadSettings.onlyExternalPowerDownloads, value);
                OnPropertyChanged("ExternalPowerOnly");
            }
        }
        public TransferPreferences TransferPrefs() {
            if (!WifiOnly && !ExternalPowerOnly) {
                return TransferPreferences.AllowCellularAndBattery;
            }
            if (!WifiOnly) {
                return TransferPreferences.AllowCellular;
            }
            if (!ExternalPowerOnly) {
                return TransferPreferences.AllowBattery;
            }
            // WP8 nonsense default: only allow transfer when connected to external power and wifi
            return TransferPreferences.None;
        }

    }
}
