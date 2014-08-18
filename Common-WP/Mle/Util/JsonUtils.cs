using Mle.IO;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;

namespace Mle.Util {
    /// <summary>
    /// Uses Json.NET.
    /// </summary>
    public class JsonUtils : JsonUtilsBase {
        private static JsonUtils instance = null;
        public static JsonUtils Instance {
            get {
                if(instance == null)
                    instance = new JsonUtils();
                return instance;
            }
        }

        public static T DeserializeFile<T>(string filePath) {
            return FileUtils.WithStorage<T>(s => {
                return deserialize<T>(s, filePath);
            });
        }
        public T DeserializeFileOrElse<T>(string filePath, T orElse = default(T)) {
            return FileUtils.WithStorage<T>(s => {
                if(s.FileExists(filePath)) {
                    return deserialize<T>(s, filePath);
                } else {
                    return orElse;
                }
            });
        }
        private static T deserialize<T>(IsolatedStorageFile s, string filePath) {
            using(var fileStream = s.OpenFile(filePath, FileMode.Open))
            using(var fileReader = new StreamReader(fileStream))
            using(var jsonReader = new JsonTextReader(fileReader)) {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(jsonReader);
            }
        }
        public override async Task WithWriter(string filePath, Func<Stream, Task> writingCode) {
            await FileUtils.WithStorage(async s => {
                using(var fileStream = s.OpenFile(filePath, FileMode.OpenOrCreate)) {
                    await writingCode(fileStream);
                }
            });
        }
        public void WithWriterSync(string filePath, Action<Stream> writingCode) {
            FileUtils.WithStorage(s => {
                using(var fileStream = s.OpenFile(filePath, FileMode.OpenOrCreate)) {
                    writingCode(fileStream);
                }
            });
        }
        public void SerializeToFileSync(object obj, string filePath) {
            WithWriterSync(filePath, stream => SerializeToFile(obj, stream));
        }
    }
}
