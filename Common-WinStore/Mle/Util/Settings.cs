using System;
using Windows.Storage;

namespace Mle.Util {
    public class Settings : ISettingsManager {
        private static Settings instance = null;
        public static Settings Instance {
            get {
                if (instance == null)
                    instance = new Settings();
                return instance;
            }
        }
        private ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
        /// <summary>
        /// Throws if the value exceeds whatever roaming settings quota, 
        /// the limit seems to be about 4000 chars.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Save<T>(string key, T value) {
            roamingSettings.Values[key] = value;
        }

        public T Load<T>(string key, T def = default(T)) {
            return LoadOrDefault<T>(() => roamingSettings.Values[key], def);
        }
        public T LoadOrDefault<T>(Func<object> loader, T def = default(T)) {
            var value = loader();
            if (value != null) {
                return (T)value;
            } else {
                return def;
            }
        }
    }
}
