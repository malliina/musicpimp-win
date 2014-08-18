using Mle.IO;
using Mle.Util;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Mle.Roaming.Network {
    public class RoamingSettings : Settings {
        public StorageFolder RoamingFolder { get; private set; }
        public RoamingSettings() {
            RoamingFolder = ApplicationData.Current.RoamingFolder;
        }

        public async Task<T> LoadFromFile<T>(string fileName, T def = default(T)) {
            var maybeFile = await RoamingFolder.GetFileIfExists(fileName);
            if (maybeFile != null) {
                var contents = await FileIO.ReadTextAsync(maybeFile);
                var ret = Json.Deserialize<T>(contents);
                return ret;
            } else {
                return def;
            }
        }
        public async Task SaveToFile(string fileName, object value) {
            var file = await RoamingFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            var contents = Json.SerializeToString(value);
            await FileIO.WriteTextAsync(file, contents);
        }

        public string LoadString(string key) {
            return ApplicationData.Current.RoamingSettings.Values[key] as string;
        }
        private void SaveAsString(string key, object data, Func<object, string> f) {
            Save(key, f(data));
        }
        public void SaveJson(string key, object data, bool compress = false) {
            Func<object, string> serialize = o => Json.SerializeToString(o);
            Func<object, string> f = null;
            if (compress) {
                f = o => ZipToBase64(serialize(o));
            } else {
                f = serialize;
            }
            SaveAsString(key, data, f);
        }
        private T LoadAsJson<T>(string key, Func<string, string> jsonifier, T def = default(T)) {
            var stored = LoadString(key);
            if (stored != null) {
                var json = jsonifier(stored);
                return Json.Deserialize<T>(json);
            } else {
                return def;
            }

        }
        public T LoadJson<T>(string key, T def = default(T), bool decompress = false) {
            Func<string, string> f = null;
            if (decompress) {
                f = s => UnzipFromBase64(s);
            } else {
                f = s => s;
            }
            return LoadAsJson(key, f, def);
        }

        public static string ZipToBase64(string str) {
            return Convert.ToBase64String(Zip(str));
        }
        public static string UnzipFromBase64(string base64zip) {
            return Unzip(Convert.FromBase64String(base64zip));
        }

        public static byte[] Zip(string str) {
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream()) {
                using (var gs = new GZipStream(mso, CompressionMode.Compress)) {
                    msi.CopyTo(gs);
                }
                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes) {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream()) {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress)) {
                    gs.CopyTo(mso);
                }
                var arr = mso.ToArray();
                return Encoding.UTF8.GetString(arr, 0, arr.Length);
            }
        }
    }
}
