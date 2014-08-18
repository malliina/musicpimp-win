using System;

namespace Mle.Network {
    public class DownloadItem {
        public Uri RemoteUri { get; private set; }
        public string Destination { get; private set; }
        public DownloadItem(string uri, string dest) {
            RemoteUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            Destination = dest;
        }
    }
}
