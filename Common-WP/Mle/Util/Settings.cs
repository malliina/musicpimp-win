using System.IO.IsolatedStorage;

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
        public void SaveAsJson<T>(string key, T value) {
            Save<string>(key,Json.SerializeToString(value));
        }
        public T LoadAsJson<T>(string key, T def = default(T)) {
            var jsonContent = Load<string>(key, null);
            if(jsonContent != null) {
                return Json.Deserialize<T>(jsonContent);
            } else {
                return def;
            }
        }

        public void Save<T>(string key, T value) {
            IsolatedStorageSettings.ApplicationSettings[key] = value;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }
        public T Load<T>(string key, T def = default(T)) {
            T value;
            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue<T>(key, out value)) {
                return value;
            } else {
                return def;
            }
        }
    }
    
}
