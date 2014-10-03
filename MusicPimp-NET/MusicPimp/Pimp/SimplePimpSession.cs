using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using System.Threading;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Pimp {
    public class SimplePimpSession : SessionBase {
        public static readonly string PlaybackSocketResource = "/ws/playback";
        public const string JSONv18 = "application/vnd.musicpimp.v18+json";
        public string SocketResource { get; protected set; }

        public SimplePimpSession(MusicEndpoint settings, bool acceptCompression = true)
            : base(settings, JSONv18, acceptCompression) {
                SocketResource = PlaybackSocketResource;
        }
        public override Task TestConnectivity() {
            return TestPing();
        }
        // failures should throw, so parameterizing the returned task is suspect, TODO document and/or fix
        public Task Ping() {
            return ToJson<FailureResponse>("/ping");
        }
        public Task<FailureResponse> Ping(CancellationToken token) {
            return Ping<FailureResponse>("/ping", token);
        }
        public virtual Task TestPing() {
            return Ping();
        }
        protected Task<T> Ping<T>(string resource, CancellationToken cancellationToken) {
            return Client.GetJson<T>(resource, cancellationToken);
        }
        public Task<string> PostCommand(JsonContent command, string resource) {
            return Client.PostJson(command.Json(), resource);
        }
        public Task<string> PostSimple(string command, string resource) {
            return PostCommand(new SimpleCommand(command), resource);
        }
        public Task<string> PostValue(string command, int value, string resource) {
            return PostCommand(new GenericCommand<int>(command, value), resource);
        }
        public Task<string> PostTrack(string command, string track, string resource) {
            return PostCommand(new TrackCommand(command, track), resource);
        }

        public Task<T> ToJson<T>(string resource) {
            return Client.GetJson<T>(resource);
        }
        public Task<StatusPimpResponse> StatusCall() {
            return ToJson<StatusPimpResponse>("/playback");
        }
        public Task<VersionResponse> PingAuth() {
            return ToJson<VersionResponse>("/pingauth");
        }
    }
}
