
namespace Mle.MusicPimp.Exceptions {
    public class TimeoutPimpException : ConnectivityException {
        public TimeoutPimpException(string msg)
            : base(msg) {
        }
    }
}
