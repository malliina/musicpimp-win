
namespace Mle.Exceptions {
    public class NotFoundException : ServerResponseException {
        public NotFoundException(string msg) : base(msg) { }
        public NotFoundException() : this(string.Empty) { }
    }
}
