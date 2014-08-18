using Mle.IO;
using Mle.IO.Local;
using Mle.MusicPimp.Network;

namespace Mle.Network {
    public abstract class StoreUploadRequest : UploadRequest {

        public StoreUploadRequest(string baseUri, string username, string password) :
            base(baseUri, username, password) { }

        public override IPathHelper Paths {
            get { return PathHelper.Instance; }
        }
    }
}
