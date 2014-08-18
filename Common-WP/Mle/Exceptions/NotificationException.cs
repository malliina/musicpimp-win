using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mle.Exceptions {
    public class NotificationException : FriendlyException {
        public NotificationException(string msg) : base(msg) { }
    }
}
