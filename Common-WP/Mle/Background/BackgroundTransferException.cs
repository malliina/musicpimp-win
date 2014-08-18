using System;

namespace Mle.Background {
    public class BackgroundTransferException : Exception {
        public BackgroundTransferException(string msg) : base(msg) { }
    }
}
