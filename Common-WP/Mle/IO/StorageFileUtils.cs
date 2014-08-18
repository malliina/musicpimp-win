using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace Mle.IO {
    public class StorageFileUtils {
        public StorageFolder Folder { get; private set; }

        public StorageFileUtils(StorageFolder folder) {
            Folder = folder;
        }
        public Task<IEnumerable<StorageFile>> ListFiles(string relativeFolder, bool recursive = false) {
            return Folder.ListFiles(relativeFolder, recursive);
        }
        public virtual Uri UriFor(IStorageFile file) {
            return new Uri(file.Path);
        }
    }
}
