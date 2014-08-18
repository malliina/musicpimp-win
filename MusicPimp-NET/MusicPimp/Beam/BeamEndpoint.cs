using Mle.MusicPimp.ViewModels;
namespace Mle.MusicPimp.Beam {

    public class BeamEndpoint : MusicEndpoint {
        public static readonly string BeamName = "MusicBeamer";

        public BeamEndpoint(string host, int port, string user, bool useTls) {
            Name = BeamName;
            Server = host;
            Port = port;
            Username = user;
            Password = "beam";
            EndpointType = EndpointTypes.Beam;
            Protocol = useTls ? Protocols.https : Protocols.http;
        }
    }
}
