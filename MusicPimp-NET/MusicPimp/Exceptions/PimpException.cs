using System;

namespace Mle.MusicPimp.Exceptions {
    public class PimpException : Exception {
        public PimpException(string msg)
            : base(msg) {
        }
    }
}
