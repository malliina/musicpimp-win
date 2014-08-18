using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mle.Exceptions {
    /// <summary>
    /// An exception with a user-friendly Message property: the message is crafted in
    /// a way that it can safely be shown to a non-technical user.
    /// </summary>
    public class FriendlyException : Exception {
        public FriendlyException(string msg) : this(msg, null) { }
        public FriendlyException(string msg, Exception e) : base(msg, e) { }
    }
}
