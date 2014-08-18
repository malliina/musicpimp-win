
using System;
namespace Mle.Exceptions {
    public class ServerResponseException : Exception {
        public ServerResponseException(string msg) : base(msg) { }
    }
}
