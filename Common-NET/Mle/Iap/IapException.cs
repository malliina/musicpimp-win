using System;

namespace Mle.Iap {
    public class IapException : Exception {
        public IapException(string msg, Exception cause) : base(msg, cause) { }
        public IapException(string msg) : base(msg) { }
    }
}
