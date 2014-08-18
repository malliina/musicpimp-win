using Mle.MusicPimp.Util;
using Mle.Util;

namespace Mle.MusicPimp.ViewModels {
    /// <summary>
    /// </summary>
    public class PhoneSetting {
        private string settingName;
        private ISettingsManager settingsManager;

        public PhoneSetting(SettingKey endpointKey) {
            settingName = endpointKey.ToString();
            settingsManager = Settings.Instance;
        }
        public int LoadIndex() {
            return settingsManager.Load<int>(settingName);
        }
        public void SaveEndpoint(int value) {
            settingsManager.Save<int>(settingName, value);
        }
    }
}
