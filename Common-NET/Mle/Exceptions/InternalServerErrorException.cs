
namespace Mle.Exceptions {
    public class InternalServerErrorException : ServerResponseException {
        public InternalServerErrorException(string msg) : base(msg) { }
        public InternalServerErrorException() : base("Internal server error") { }
    }
}
