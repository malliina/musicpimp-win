using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using System;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Pimp {

    public class PhonePimpSession : PimpSession {
        public PhonePimpSession(MusicEndpoint settings, bool acceptCompression = true)
            : base(settings, acceptCompression) {
        }
        public override IDownloader BackgroundDownloader {
            get { return PimpViewModel.Instance.Downloader; }
        }
        protected override LocalMusicLibrary LocalLibrary {
            get { return PhoneLocalLibrary.Instance; }
        }
        protected override Task EnsureHasDuration(MusicItem track) {
            return DurationHelper.SetDurationIfZero(track);
        }
        /// <summary>
        /// Uploads the song to the given resource. 
        /// 
        /// If the song is not locally available, it is first downloaded to the device, then uploaded.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="song"></param>
        /// <returns></returns>
        //public override async Task<string> Upload(MusicItem song, string resource) {
        //    Uri maybeLocalUri = await LocalLibrary.LocalUriIfExists(song);
        //    if(maybeLocalUri == null) {
        //        await BackgroundDownloader.DownloadAsync(song, Username, Password);
        //    }
        //    await DurationHelper.SetDurationIfZero(song);
        //    // throws OOM with tranceport
        //    var request = BuildUploadRequest(song);
        //    return await request.UploadAsync(resource);
        //}
        //public override UploadRequest NewUploadRequest() {
        //    return new PhoneUploadRequest(BaseUri, Username, Password);
        //}
        //protected override string LocalPathTo(MusicItem song) {
        //    return LocalLibrary.AbsolutePathTo(song);
        //}
    }
}
