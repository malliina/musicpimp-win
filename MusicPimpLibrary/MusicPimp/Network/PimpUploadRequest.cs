using Mle.MusicPimp.Local;
using Mle.Network;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Network {
    public class PimpUploadRequest : StoreUploadRequest {
        public PimpUploadRequest(string baseUri, string username, string password)
            : base(baseUri, username, password) {
        }
        public override async Task WithFileRead(string filePath, Func<Stream, Task> op) {
            var library = await MultiFolderLibrary.Instance.FindLibrary(filePath);
            await library.FileUtil.WithFileReadAsync(filePath, op);
        }
    }
}
