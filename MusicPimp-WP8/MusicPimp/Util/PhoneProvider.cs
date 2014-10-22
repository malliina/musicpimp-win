using Mle.Iap;
using Mle.IO;
using Mle.IO.Local;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Beam;
using Mle.MusicPimp.Iap;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using Mle.Util;
using System;
using System.Net.Http.Headers;

namespace Mle.MusicPimp.Util {
    public class PhoneProvider : Provider {
        public IDownloader Downloader { get { return PimpViewModel.Instance.Downloader; } }
        public MusicItemsBase MusicItemsBase { get { return PimpViewModel.Instance; } }
        public PlayerManager PlayerManager { get { return PhonePlayerManager.Instance; } }
        public LibraryManager LibraryManager { get { return PhoneLibraryManager.Instance; } }
        public EndpointsData EndpointsData { get { return PhoneEndpoints.Instance; } }
        public NowPlayingInfo NowPlayingModel { get { return PhoneNowPlaying.Instance; } }
        public BasePlayer LocalPlayer { get { return PhoneLocalPlayer.Instance; } }
        public LocalMusicLibrary LocalLibrary { get { return PhoneLocalLibrary.Instance; } }
        public ISettingsManager SettingsManager { get { return Settings.Instance; } }
        public IPathHelper PathHelper { get { return IO.PathHelper.Instance; } }
        public FileUtilsBase FileUtils { get { return PhoneFileUtils.Instance; } }
        public IIapUtils IapHelper { get { return PimpIapUtils.Instance; } }
        public MusicLibrary NewPimpLibrary(MusicEndpoint e) {
            return new PimpLibrary(new PhonePimpSession(e));
        }
        public BasePlayer NewBeamPlayer(PimpSession session, PimpWebSocket socket) {
            return new PhoneBeamPlayer(session, socket);
        }
        public PimpSession NewPimpSession(MusicEndpoint e) {
            return new PhonePimpSession(e);
        }
        public PimpSession NewBeamSession(MusicEndpoint e) {
            return new PhoneBeamSession(e);
        }
        public WebSocketBase NewWebSocket(Uri uri, AuthenticationHeaderValue authHeader, string mediaType) {
            return new SimpleWebSocket(uri, authHeader, mediaType);
        }
        public AbstractOAuthBase NewOAuthBase() {
            return new OAuthBase();
        }
        public Platforms Platform { get { return Platforms.WinPhone; } }
    }
}
