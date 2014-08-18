using System;

namespace Mle.Util {
    public class UriInfo {
        public Uri Uri { get; private set; }
        public ulong Size { get; private set; }
        public UriInfo(Uri uri, ulong size) {
            Uri = uri;
            Size = size;
        }
    }
}
