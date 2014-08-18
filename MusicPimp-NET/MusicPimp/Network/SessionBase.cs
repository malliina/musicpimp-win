using Mle.MusicPimp.Network.Http;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using Mle.Util;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Mle.MusicPimp.Network {
    /// <summary>
    /// TODO remove type param
    /// </summary>
    public abstract class SessionBase : RemoteBase, IDisposable {
        public HttpClient Client { get; private set; }
        // For file uploads
        public HttpClient LongTimeoutClient { get; private set; }
        public bool UseCompression { get; private set; }
        private string mediaType;

        public SessionBase(MusicEndpoint settings, string mediaType, bool acceptCompression = true)
            : base(settings) {
            this.mediaType = mediaType;
            UseCompression = acceptCompression;
            Client = NewHttpClient(settings);
            LongTimeoutClient = NewHttpClient(settings);
            LongTimeoutClient.Timeout = TimeSpan.FromMinutes(10);
        }
        public virtual Uri DownloadUriFor(MusicItem track) {
            return track.Source;
        }
        private HttpClient NewHttpClient(MusicEndpoint e) {
            var handler = new HttpClientHandler();
            if(UseCompression && handler.SupportsAutomaticDecompression) {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            } else {
                handler.AutomaticDecompression = DecompressionMethods.None;
            }
            var client = new HttpClient(handler);
            client.BaseAddress = new Uri(e.Protocol + "://" + e.Server + ":" + e.Port);
            var headers = client.DefaultRequestHeaders;
            headers.Authorization = AuthorizationHeader(e.Username, e.Password);
            headers.Accept.ParseAdd(mediaType);
            return client;
        }
        
        protected virtual AuthenticationHeaderValue AuthorizationHeader(string username, string password) {
            return HttpUtil.BasicAuthHeaderValue(username, password);
        }
        protected Uri ToUri(string uriString) {
            return new Uri(uriString, UriKind.Absolute);
        }
        public void Dispose() {
            Utils.Suppress<Exception>(Client.Dispose);
            Utils.Suppress<Exception>(LongTimeoutClient.Dispose);
        }
    }
}
