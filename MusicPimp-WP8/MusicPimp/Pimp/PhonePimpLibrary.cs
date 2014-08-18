using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using Mle.ViewModels;

namespace Mle.MusicPimp.Pimp {
    public class PhonePimpLibrary : PimpLibrary {
        // TODO remove query param or document why it's necessary
        public PhonePimpLibrary(PimpSessionBase s)
            : base(s, useCredentialsInQueryParam: true) {
        }
    }
}
