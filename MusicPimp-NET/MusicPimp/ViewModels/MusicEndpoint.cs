using Mle.ViewModels;
using System;

namespace Mle.MusicPimp.ViewModels {
    public enum EndpointTypes { Local, MusicPimp, MusicPimpWeb, PimpCloud, Beam, Subsonic }
    public enum Protocols { https, http }

    public class MusicEndpoint : ViewModelBase {
        public static readonly string DEFAULT_SERVER = null;
        public static readonly string DEFAULT_USERNAME = "admin";
        public static readonly int DEFAULT_PORT = 8456;
        public static readonly Protocols DEFAULT_PROTOCOL = Protocols.http;
        public MusicEndpoint() {
            Id = Guid.NewGuid().ToString();
        }
        public MusicEndpoint(string server, string password) {
            Id = Guid.NewGuid().ToString();
            Name = server;
            Server = server;
        }
        public MusicEndpoint(MusicEndpoint blueprint) {
            Id = blueprint.Id;
            Name = blueprint.Name;
            Protocol = blueprint.Protocol;
            CloudServerID = blueprint.CloudServerID;
            Server = blueprint.Server;
            Port = blueprint.Port;
            Username = blueprint.Username;
            Password = blueprint.Password;
            EndpointType = blueprint.EndpointType;
        }
        private string id;
        public string Id {
            get { return id; }
            set { this.SetProperty(ref this.id, value); }
        }
        private string _name;
        public string Name {
            get { return _name; }
            set { this.SetProperty(ref this._name, value); }
        }
        private Protocols _protocol = DEFAULT_PROTOCOL;
        public Protocols Protocol {
            get { return _protocol; }
            set { this.SetProperty(ref this._protocol, value); }
        }
        private string _server;
        public string Server {
            get { return _server; }
            set { this.SetProperty(ref this._server, value); }
        }
        private int _port = DEFAULT_PORT;
        public int Port {
            get { return _port; }
            set { this.SetProperty(ref this._port, value); }
        }
        private string _username = DEFAULT_USERNAME;
        public string Username {
            get { return _username; }
            set { this.SetProperty(ref this._username, value); }
        }
        private string _password;
        public string Password {
            get { return _password; }
            set { this.SetProperty(ref this._password, value); }
        }
        private string cloudServerID;
        public string CloudServerID {
            get { return cloudServerID; }
            set { SetProperty(ref cloudServerID, value); }
        }
        private EndpointTypes _endpointType = EndpointTypes.MusicPimp;
        public EndpointTypes EndpointType {
            get { return _endpointType; }
            set {
                if(SetProperty(ref this._endpointType, value)) {
                    OnPropertyChanged("SupportsLibrary");
                }
            }
        }
        public bool SupportsLibrary {
            // web-based players do not support music libraries
            get { return EndpointType != EndpointTypes.Beam && EndpointType != EndpointTypes.MusicPimpWeb; }
        }
        public bool CanReceiveMusic {
            get { return EndpointType != EndpointTypes.Subsonic; }
        }
        public string BaseUri() {
            return Protocol + "://" + Server + ":" + Port;
        }
        public Uri Uri() {
            return new Uri(BaseUri(), UriKind.Absolute);
        }
    }
}