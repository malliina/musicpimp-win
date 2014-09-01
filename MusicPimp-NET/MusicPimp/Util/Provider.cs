using Mle.Iap;
using Mle.IO;
using Mle.IO.Local;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using Mle.Util;
using System;

namespace Mle.MusicPimp.Util {
    public class ProviderService : Provider {
        private static ProviderService instance = null;
        public static ProviderService Instance {
            get {
                if(instance == null)
                    instance = new ProviderService();
                return instance;
            }
        }
        private Provider impl;
        public void Register(Provider provider) {
            impl = provider;
        }
        public MusicItemsBase MusicItemsBase { get { return impl.MusicItemsBase; } }
        public PlayerManager PlayerManager { get { return impl.PlayerManager; } }
        public LibraryManager LibraryManager { get { return impl.LibraryManager; } }
        public EndpointsData EndpointsData { get { return impl.EndpointsData; } }
        public NowPlayingInfo NowPlayingModel { get { return impl.NowPlayingModel; } }
        public LocalMusicLibrary LocalLibrary { get { return impl.LocalLibrary; } }
        public BasePlayer LocalPlayer { get { return impl.LocalPlayer; } }
        public ISettingsManager SettingsManager { get { return impl.SettingsManager; } }
        public IPathHelper PathHelper { get { return impl.PathHelper; } }
        //public FileUtilsBase FileUtils { get { return impl.FileUtils; } }
        public IIapUtils IapHelper { get { return impl.IapHelper; } }
        public MusicLibrary NewPimpLibrary(MusicEndpoint e) {
            return impl.NewPimpLibrary(e);
        }
        public BasePlayer NewBeamPlayer(PimpSession session, PimpWebSocket socket) {
            return impl.NewBeamPlayer(session, socket);
        }
        public PimpSession NewPimpSession(MusicEndpoint e) {
            return impl.NewPimpSession(e);
        }
        public PimpSession NewBeamSession(MusicEndpoint e) {
            return impl.NewBeamSession(e);
        }
        public WebSocketBase NewWebSocket(Uri uri, string userName, string password, string mediaType) {
            return impl.NewWebSocket(uri, userName, password, mediaType);
        }
        public AbstractOAuthBase NewOAuthBase() {
            return impl.NewOAuthBase();
        }
        public IDownloader Downloader { get { return impl.Downloader; } }
    }
    public interface Provider {
        MusicItemsBase MusicItemsBase { get; }
        PlayerManager PlayerManager { get; }
        LibraryManager LibraryManager { get; }
        EndpointsData EndpointsData { get; }
        NowPlayingInfo NowPlayingModel { get; }
        BasePlayer LocalPlayer { get; }
        LocalMusicLibrary LocalLibrary { get; }
        ISettingsManager SettingsManager { get; }
        IPathHelper PathHelper { get; }
        IIapUtils IapHelper { get; }
        IDownloader Downloader { get; }
        MusicLibrary NewPimpLibrary(MusicEndpoint e);
        BasePlayer NewBeamPlayer(PimpSession session, PimpWebSocket socket);
        PimpSession NewPimpSession(MusicEndpoint e);
        PimpSession NewBeamSession(MusicEndpoint e);
        WebSocketBase NewWebSocket(Uri uri, string userName, string password, string mediaType);
        AbstractOAuthBase NewOAuthBase();
    }
}
