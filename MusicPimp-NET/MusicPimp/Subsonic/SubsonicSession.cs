using Mle.Exceptions;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using System;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Subsonic {
    public enum SubsonicMethod { ping, getIndexes, getMusicDirectory, download, jukeboxControl }
    /// <summary>
    /// TOOD: fix this code.
    /// </summary>
    public class SubsonicSession : SubsonicBase {

        public SubsonicSession(MusicEndpoint settings)
            : base(settings) {
        }

        public virtual Task<string> ApiCall(Uri uri) {
            return Client.GetString(uri);
        }
        public override async Task TestConnectivity() {
            var response = await WebTask<SubsonicResponse>(pingAsync(), timeout: TimeSpan.FromMilliseconds(5000));
            if(response.status == "ok") {
                return;
            } else {
                throw new ServerResponseException("The server returned the following error: " + response.error.message + ".");
            }
        }
        public Task<SubsonicResponse> pingAsync() {
            return jsonCallAsync<SubsonicResponse, SubsonicResponseContainer>("ping");
        }
        public Task<IndexesResponse> indexesAsync() {
            return jsonCallAsync<IndexesResponse, SubsonicIndexesContainer>("getIndexes");
        }
        public Task<DirectoryResponse> musicFilesAsync(int id) {
            return jsonCallAsync<DirectoryResponse, SubsonicDirectoryContainer>("getMusicDirectory", id);
        }
        public Task<string> serverAddToPlaylistAsync(int id) {
            return JukeboxCall("&action=add&id=" + id);
        }
        public Task<string> serverRemoveFromPlaylistAsync(int index) {
            return JukeboxCall("&action=remove&index=" + index);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">0-based index of song in playlist to play</param>
        public Task<string> serverSkipToPlaylistIndexAsync(int index, double offset = 0) {
            return JukeboxCall("&action=skip&index=" + index + "&offset=" + (int)offset);
        }

        public Task<string> serverSetPlaylistAsync(int id) {
            return JukeboxCall("&action=set&id=" + id);
        }

        public Task<JukeboxPlaylistResponse> serverGetPlaylistAsync() {
            return jsonCallAsync<JukeboxPlaylistResponse, JukeboxControlContainer>("jukeboxControl", "&action=get");
        }

        public Task<string> serverStopAsync() {
            return JukeboxCall("&action=stop");
        }

        public Task<string> serverPlayAsync() {
            return JukeboxCall("&action=start");
        }
        public Task<JukeboxStatusResponse> serverStatus() {
            return jsonCallAsync<JukeboxStatusResponse, JukeboxStatusContainer>("jukeboxControl", "&action=status");
        }
        public Task<SearchResponse> Search(string term, int limit) {
            return jsonCallAsync<SearchResponse, SearchContainer>("search2", "&query=" + term + "&songCount=" + limit + "&artistCount=0&albumCount=0");
        }
        public Task<string> serverSetVolumeAsync(double newVolume) {
            var formattedVolume = newVolume.ToString().Replace(',', '.');
            return JukeboxCall("&action=setGain&gain=" + formattedVolume);
        }
        private Task<string> JukeboxCall(string param) {
            return ApiCallAsync("jukeboxControl", param);
        }
        // helpers
        private async Task<T> jsonCallAsync<T, U>(string resource)
            where T : SubsonicResponse
            where U : ISubsonicResponseContainer<T> {
            var httpResponse = await ApiCallAsync(resource);
            return parseResponse<T, U>(httpResponse);
        }
        private async Task<T> jsonCallAsync<T, U>(string resource, int id)
            where T : SubsonicResponse
            where U : ISubsonicResponseContainer<T> {
            var httpResponse = await ApiCallAsync(resource, id);
            return parseResponse<T, U>(httpResponse);
        }
        private async Task<T> jsonCallAsync<T, U>(string resource, string parameters)
            where T : SubsonicResponse
            where U : ISubsonicResponseContainer<T> {
            var httpResponse = await ApiCallAsync(resource, parameters);
            return parseResponse<T, U>(httpResponse);
        }
        public Task<string> ApiCallAsync(string resource, string parameters) {
            var uri = UriFor(resource, parameters);
            return ApiCall(uri);
        }
        public Task<string> ApiCallAsync(string resource) {
            var uri = UriFor(resource);
            return ApiCall(uri);
        }

        public Task<string> ApiCallAsync(string resource, int id) {
            var uri = UriFor(resource, id);
            return ApiCall(uri);
        }
        protected Uri UriFor(string resource) {
            return ToUri(UriString(resource));
        }
    }
}
