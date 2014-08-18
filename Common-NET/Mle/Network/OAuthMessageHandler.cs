using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Mle.Network {
    /// <summary>
    /// Basic DelegatingHandler that creates an OAuth authorization header based on the OAuthBase
    /// class downloaded from http://oauth.net
    /// </summary>
    public class OAuthMessageHandler : DelegatingHandler {
        private AbstractOAuthBase _oAuthBase;
        private OAuthCredentials creds;

        public OAuthMessageHandler(AbstractOAuthBase oAuthBase, OAuthCredentials credentials, HttpMessageHandler innerHandler)
            : base(innerHandler) {
            creds = credentials;
            _oAuthBase = oAuthBase;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            // Compute OAuth header 
            string normalizedUri;
            string normalizedParameters;
            string authHeader;

            string signature = _oAuthBase.GenerateSignature(
                request.RequestUri,
                creds.ConsumerKey,
                creds.ConsumerSecret,
                creds.AccessToken,
                creds.AccessTokenSecret,
                request.Method.Method,
                _oAuthBase.GenerateTimeStamp(),
                _oAuthBase.GenerateNonce(),
                out normalizedUri,
                out normalizedParameters,
                out authHeader);

            request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader);
            return base.SendAsync(request, cancellationToken);
        }
    }
}