using Mle.MusicPimp.Beam;
using Mle.MusicPimp.ViewModels;
using PCLWebUtility;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Mle.MusicPimp.Audio;

namespace Mle.MusicPimp.Pimp {

    public class PimpSessionBase : SimplePimpSession {

        public static readonly Version MinimumServerVersion = new Version(1, 8, 0);
        public static readonly Version HttpsSupportingVersion = new Version(2, 1, 0);
        public static readonly Version SearchSupportingVersion = new Version(2, 5, 0);
        public static readonly Version AlarmsSupportingVersion = new Version(2, 5, 3);

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
        private Uri TrackUri(string track, bool useCredentialsInQueryParam, string subPath) {
            //var encodedTrack = WebUtility.UrlEncode(track);
            var credQueryParam = useCredentialsInQueryParam ? QueryCredentials() : String.Empty;
            // "/tracks/"
            return new Uri(BaseUri + subPath + track + credQueryParam, UriKind.Absolute);
        }

        protected virtual string QueryCredentials() {
            return "?u=" + Username + "&p=" + Password;
        }
        public override Uri DownloadUriFor(MusicItem track) {
            return DownloadUriFor(track);
        }
        public Uri DownloadUriFor(string track, bool useCredentialsInQueryParam = true) {
            return TrackUri(track, useCredentialsInQueryParam, "/downloads/");
        }
        public Uri PlaybackUriFor(string track, bool useCredentialsInQueryParam = true) {
            return TrackUri(track, useCredentialsInQueryParam, "/tracks/");
        }
        public Task<FoldersPimpResponse> RootContentAsync() {
            return ToJson<FoldersPimpResponse>("/folders");
        }
        public Task<FoldersPimpResponse> ContentsIn(string folderId) {
            return ToJson<FoldersPimpResponse>("/folders/" + folderId);
        }
        public async Task<IEnumerable<MusicItem>> Search(string term) {
            var jsonResponse = await ToJson<IEnumerable<PimpTrack>>("/search?term=" + term + "&limit=100");
            return jsonResponse
                .Select(item => AudioConversions.PimpTrackToMusicItem(item, PlaybackUriFor(item.id), Username, Password, IsCloud ? CloudServerID : null))
                .ToList();
        }
        public Task<VersionResponse> PingAuth(CancellationToken token) {
            return Ping<VersionResponse>("/pingauth", token);
        }
    }
}
