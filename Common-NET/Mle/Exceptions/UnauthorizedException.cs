using System;

namespace Mle.Exceptions {
    public class UnauthorizedException : ServerResponseException {
        public UnauthorizedException(string msg) : base(msg) { }
        public UnauthorizedException() : this(String.Empty) { }
    }
}
