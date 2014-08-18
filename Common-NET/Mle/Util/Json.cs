using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Mle.Util {
    public class Json {
        public static string SerializeToString(object obj) {
            return JsonConvert.SerializeObject(obj);
        }
        public static T Deserialize<T>(string json) {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

    public abstract class JsonUtilsBase {
        public void SerializeToFile(object obj, Stream fileStream) {
            using(var fileWriter = new StreamWriter(fileStream))
            using(var jsonWriter = new JsonTextWriter(fileWriter)) {
                var serializer = new JsonSerializer();
                serializer.Serialize(jsonWriter, obj);
            }
        }
        public Task SerializeToFile(object obj, string filePath) {
            return WithWriter(filePath, stream => TaskEx.Run(() => SerializeToFile(obj, stream)));
        }

        public abstract Task WithWriter(string filePath, Func<Stream, Task> writingCode);


        //public abstract T DeserializeFileOrElse<T>(string filePath, T orElse = default(T));
    }
}
