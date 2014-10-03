
namespace Mle.MusicPimp.ViewModels {
    public class CloudEndpoint : MusicEndpoint {
        public static readonly string SERVER = "cloud.musicpimp.org";
        //public static readonly string SERVER = "localhost";
        public static readonly int PORT = 443;
        //public static readonly int PORT = 9000;
        public static readonly Protocols PROTOCOL = Protocols.https;
        //public static readonly Protocols PROTOCOL = Protocols.http;
        public CloudEndpoint() {
            Protocol = PROTOCOL;
            Server = SERVER;
            Port = PORT;
            EndpointType = EndpointTypes.PimpCloud;
        }
        public CloudEndpoint(string name, string pimpID, string username, string password)
            : this() {
            Name = name;
            CloudServerID = pimpID;
            Username = username;
            Password = password;
        }
    }
}
