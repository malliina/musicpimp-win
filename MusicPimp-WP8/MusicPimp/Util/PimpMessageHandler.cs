using Mle.Messaging;
using System;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Util {
    public class PimpMessageHandler : MessageHandler, IPimpMessageHandler {
        public async Task HandleConnectivityStatus(bool connected) {
            if(!connected) {
                await Handle(new SimpleMessage("Unable to connect to the music source. Only local tracks will be shown in the library. Check your settings."));
            } else {

            }
        }
    }
}
