using System;

namespace Mle.Network {
    public class Downloadable {
        public Uri Source { get; private set; }
        public string Destination { get; private set; }
        public Downloadable(Uri source, string dest) {
            Source = source;
            Destination = dest;
        }
    }
}
