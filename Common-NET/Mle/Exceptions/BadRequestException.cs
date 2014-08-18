
namespace Mle.Exceptions {
    public class BadRequestException : ServerResponseException {
        public string Content { get; private set; }
        public BadRequestException(string msg, string content)
            : base(msg) {
            Content = content;
        }
    }
}
