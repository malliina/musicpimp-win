using Mle.MusicPimp.Local;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Util;
using Mle.MusicPimp.ViewModels;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Pimp {
    public abstract class PimpSession : PimpSessionBase {
        protected static readonly string TrackHeader = "Track";
        protected abstract LocalMusicLibrary LocalLibrary { get; }
        public abstract IDownloader BackgroundDownloader { get; }
        protected abstract Task EnsureHasDuration(MusicItem track);

        public PimpSession(MusicEndpoint settings, bool acceptCompression = true)
            : base(settings, acceptCompression) {
        }
        public async Task<string> Upload(MusicItem song, string resource) {
            await DownloadIfNecessary(song);
            await EnsureHasDuration(song);
            // TODO refactor to HttpClient extension method or some such
            using(var content = new MultipartFormDataContent()) {
                // the trickery with quotes is due to bugs in the recipient of the uploaded content, Play Framework 2
                // see http://stackoverflow.com/questions/17900934/httpclient-uploading-multipartformdata-to-play-2-framework
                foreach(var param in content.Headers.ContentType.Parameters.Where(param => param.Name.Equals("boundary")))
                    param.Value = param.Value.Replace("\"", String.Empty);
                return await LocalLibrary.WithStream<string>(song, async stream => {
                    var fileName = ProviderService.Instance.PathHelper.FileNameOf(song.Path);
                    content.Add(new StreamContent(stream), "\"file\"", "\"" + fileName + "\"");
                    content.Headers.Add(TrackHeader, JsonConvert.SerializeObject(new PimpTrack(song)));
                    using(var httpResponse = await LongTimeoutClient.PostAsync(resource, content)) {
                        return await httpResponse.Content.ReadAsStringAsync();
                    }
                });
            }
        }
        private async Task DownloadIfNecessary(MusicItem track) {
            var maybeLocalUri = await LocalLibrary.LocalUriIfExists(track);
            if(maybeLocalUri == null) {
                await BackgroundDownloader.DownloadAsync(track);
            }
        }
    }
}
