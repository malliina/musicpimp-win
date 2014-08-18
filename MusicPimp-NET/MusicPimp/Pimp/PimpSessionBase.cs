using Mle.MusicPimp.Beam;
using Mle.MusicPimp.ViewModels;
using PCLWebUtility;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Pimp {

    public class PimpSessionBase : SimplePimpSession {

        public static readonly Version MinimumServerVersion = new Version(1, 8, 0);
        public static readonly Version HttpsSupportingVersion = new Version(2, 1, 0);
        public static readonly Version AlarmsSupportingVersion = new Version(2, 3, 5);

        public PimpSessionBase(MusicEndpoint settings, bool acceptCompression = true)
            : base(settings, acceptCompression) {
        }

        public override Task TestConnectivity() {
            return GetPingAuth();
        }
        public Task<VersionResponse> GetPingAuth() {
            return WebTask<VersionResponse>(PingAuth(), timeout: TimeSpan.FromSeconds(10000));
        }

        /// <summary>
        /// Cannot put the credentials into the header as the backgroundaudioplayer only accepts a URI.
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        public Uri TrackUri(string trackId, bool useCredentialsInQueryParam, string subPath) {
            var encodedTrack = WebUtility.UrlEncode(trackId);
            var credQueryParam = useCredentialsInQueryParam ? ("?u=" + Username + "&p=" + Password) : String.Empty;
            // "/tracks/"
            return new Uri(BaseUri + subPath + encodedTrack + credQueryParam, UriKind.Absolute);
        }
        public override Uri DownloadUriFor(MusicItem track) {
            return DownloadUriFor(track.Id);
        }
        public Uri DownloadUriFor(string trackId, bool useCredentialsInQueryParam = true) {
            return TrackUri(trackId, useCredentialsInQueryParam, "/downloads/");
        }
        public Uri PlaybackUriFor(string trackId, bool useCredentialsInQueryParam = true) {
            return TrackUri(trackId, useCredentialsInQueryParam, "/tracks/");
        }
        public Task<FoldersPimpResponse> RootContentAsync() {
            return ToJson<FoldersPimpResponse>("/folders");
        }
        public Task<FoldersPimpResponse> ContentsIn(string folderId) {
            return ToJson<FoldersPimpResponse>("/folders/" + folderId);
        }
        public Task<FoldersPimpResponse> Search(string searchText) {
            return ToJson<FoldersPimpResponse>("/search/" + searchText);
        }
        public Task<VersionResponse> PingAuth(CancellationToken token) {
            return Ping<VersionResponse>("/pingauth", token);
        }
    }
}
