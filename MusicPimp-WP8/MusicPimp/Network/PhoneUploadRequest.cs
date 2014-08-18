using Mle.IO;
using Mle.IO.Local;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Network;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Mle.Network {
    public class PhoneUploadRequest : UploadRequest {

        public PhoneUploadRequest(string baseUri, string username, string password) :
            base(baseUri, username, password) { }

        public override IPathHelper Paths {
            get {
                return PathHelper.Instance;
            }
        }

        public override Task WithFileRead(string filePath, Func<Stream, Task> op) {
            return PhoneLocalLibrary.Instance.FileUtil.WithFileReadAsync(filePath, op);
        }
    }
}
