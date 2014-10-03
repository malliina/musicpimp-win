using Mle.MusicPimp.Audio;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using System;
using System.Net.Http.Headers;
using System.Text;

namespace Mle.MusicPimp.Pimp {
    public class CloudSession : PimpSessionBase {
        public static readonly string CloudPlaybackSocketResource = "/mobile/ws";
        public static readonly string Pimp = "Pimp";
        public static readonly string SERVER_KEY = "s";
        public CloudSession(MusicEndpoint e)
            : base(e) {
            SocketResource = CloudPlaybackSocketResource;
        }
        public override AuthenticationHeaderValue AuthHeader(MusicEndpoint e) {
            Byte[] credBytes = Encoding.UTF8.GetBytes(String.Format("{0}:{1}:{2}", e.CloudServerID, e.Username, e.Password));
            var encoded = Convert.ToBase64String(credBytes);
            return new AuthenticationHeaderValue(Pimp, encoded);
        }
        protected override string QueryCredentials() {
            return base.QueryCredentials() + "&" + CloudSession.SERVER_KEY + "=" + CloudServerID;
        }
    }
}
