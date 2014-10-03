using Mle.Iap;
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
    public class WinStoreProvider : Provider {
        public IDownloader Downloader {
            get { return PimpStoreDownloader.Instance; }
        }
        public MusicItemsBase MusicItemsBase {
            get { return MusicItemsModel.Instance; }
        }
        public PlayerManager PlayerManager {
            get { return StorePlayerManager.Instance; }
        }
        public LibraryManager LibraryManager {
            get { return StoreLibraryManager.Instance; }
        }
        public EndpointsData EndpointsData {
            get { return StoreEndpoints.Instance; }
        }
        public NowPlayingInfo NowPlayingModel {
            get { return StoreNowPlaying.Instance; }
        }
        public BasePlayer LocalPlayer {
            get { return StoreLocalPlayer.Instance; }
        }
        public LocalMusicLibrary LocalLibrary {
            get { return MultiFolderLibrary.Instance; }
        }
        public ISettingsManager SettingsManager {
            get { return Mle.Util.Settings.Instance; }
        }
        public IPathHelper PathHelper {
            get { return Mle.IO.PathHelper.Instance; }
        }
        //public FileUtilsBase FileUtils {
        //    get { return StoreFileUtils.; }
        //}
        public IIapUtils IapHelper {
            get { return PimpIapUtils.Instance; }
        }
        public MusicLibrary NewPimpLibrary(MusicEndpoint e) {
            return new PimpLibrary(new StorePimpSession(e));
        }
        public BasePlayer NewBeamPlayer(PimpSession session, PimpWebSocket socket) {
            return new StoreBeamPlayer(session, socket);
        }
        public PimpSession NewPimpSession(MusicEndpoint e) {
            return new StorePimpSession(e);
        }
        public PimpSession NewBeamSession(MusicEndpoint e) {
            return new StorePimpSession(e);
        }
        public WebSocketBase NewWebSocket(Uri uri, AuthenticationHeaderValue authHeader, string mediaType) {
            return new SimpleWebSocket(uri, authHeader, mediaType);
        }
        public AbstractOAuthBase NewOAuthBase() {
            return new OAuthBase();
        }
        public Platforms Platform { get { return Platforms.WinStore; } }
    }
}
