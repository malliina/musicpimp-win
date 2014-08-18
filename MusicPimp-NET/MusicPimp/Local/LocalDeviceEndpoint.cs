using Mle.MusicPimp.ViewModels;

namespace Mle.MusicPimp.Local {
    public class LocalDeviceEndpoint : MusicEndpoint {
        public LocalDeviceEndpoint() {
            Name = "this device";
            Server = "";
            Port = 0;
            Username = "";
            Password = "";
            EndpointType = EndpointTypes.Local;
        }
    }
}
