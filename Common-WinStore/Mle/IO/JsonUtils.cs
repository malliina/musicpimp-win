using Mle.Util;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Mle.IO {
    public class JsonUtils : JsonUtilsBase {
        private static JsonUtils instance = null;
        public static JsonUtils Instance {
            get {
                if (instance == null)
                    instance = new JsonUtils();
                return instance;
            }
        }
        private StorageFolder localFolder = ApplicationData.Current.LocalFolder;

        public override async Task WithWriter(string filePath, Func<Stream, Task> writingCode) {
            using (var stream = await localFolder.OpenStreamForWriteAsync(filePath, CreationCollisionOption.ReplaceExisting)) {
                await writingCode(stream);
            }
        }

        //public override T DeserializeFileOrElse<T>(string filePath, T orElse = default(T)) {
        //    StorageFile.C
        //}
    }
}
