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
    }
}
