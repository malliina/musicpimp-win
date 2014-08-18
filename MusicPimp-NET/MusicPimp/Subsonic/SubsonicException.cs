using Mle.MusicPimp.Exceptions;

namespace Mle.MusicPimp.Subsonic {
    public class SubsonicException : PimpException {
        public SubsonicException(string message) : base(message) { }
    }
}
