using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Beam {
    public class PhoneBeamSession : PhonePimpSession {
        public PhoneBeamSession(MusicEndpoint settings)
            : base(settings) {
                SocketResource = BeamPlayer.webSocketResource;
        }
    }
}
