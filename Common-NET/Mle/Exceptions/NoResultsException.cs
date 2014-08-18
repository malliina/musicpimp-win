using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mle.Exceptions {
    public class NoResultsException : Exception {
        public NoResultsException(string msg) : base(msg) { }
    }
}
