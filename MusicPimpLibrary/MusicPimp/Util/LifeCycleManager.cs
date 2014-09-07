using Mle.Common;
using Mle.Messaging;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.ViewModels;
using Mle.Tiles;
using Mle.Util;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace Mle.MusicPimp.Util {
    /// <summary>
    /// Initializes the application. The initialization procedures
    /// are called only when page navigation is being setup 
    /// because if we do stuff earlier, like in App.xaml.cs,
    /// it's difficult to display a popup or messages to the user
    /// if something goes wrong.
    /// 
    /// Initialization order:
    /// 
    /// Init()
    /// Start()
    /// InitOnNavigated()
    /// InitOnLoaded()
    /// 
    /// Navigated is fired before Loaded, but the two may eventually
    /// run concurrently.
    /// 
    /// Note: A MediaElement is embedded in the root frame, I think,
    /// and can only be accessed after a page is Loaded, I believe.
    /// 
    /// Also, the first page may initialize its viewmodel before 
    /// InitOnLoaded fires, so this is a quite delicate and 
    /// fucked up situation but should not lead to problems as far
    /// as I know.
    /// </summary>
    public class LifeCycleManager {
        private static bool IsNavigatedStageInitialized = false;
        private static bool IsLoadedStageInitialized = false;
        public static async Task Init() {
            ProviderService.Instance.Register(new WinStoreProvider());
            // MusicItemsModel needs this and is constructed before InitOnLoaded()
            await AppLocalMusicFolderFileUtils.Init();
        }
        public static async Task Start() {
            await PimpTiles.EnsureCoverFolderExists();
            MessagingService.Instance.Register(new MessageHandler());
            UiService.Instance.SetUiUtils(StoreUtil.Instance);
            //var proxyFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/WindowsStoreProxy.xml", UriKind.Absolute));
            //await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
        }
        public static async Task InitOnNavigated() {
            if(!IsNavigatedStageInitialized) {

                await MultiFolderLibrary.Instance.Init();

                MediaControlHelper.InitMediaControls();
                var tilesAndToasts = Singletons.TileManager;
                await tilesAndToasts.Init();
                //await Singletons.RegisterBackgroundTask();
                //await BackgroundTaskManager.RegisterBackgroundTask();
                await Singletons.BackgroundTasks.RegisterBackgroundTask();
                // awaiting this would await the completion of all background downloads of this app
                var downloadsTask = Utils.SuppressAsync<Exception>(PimpStoreDownloader.Instance.Utils.FollowActiveDownloadsAsync);
                //try {
                //    // awaiting this would await the completion of all background downloads of this app
                //    var t = PimpStoreDownloader.Instance.FollowActiveDownloadsAsync();
                //} catch(Exception) {

                //}
                var cacheTask = Utils.SuppressAsync<Exception>(AppLocalFolderLibrary.Instance.MaintainCacheLimit);
                //try {
                //    await AppLocalFolderLibrary.Instance.MaintainCacheLimit();
                //} catch(Exception) {
                //    //Debug.WriteLine("Unable to maintain cache: " + e.Message);
                //}
                IsNavigatedStageInitialized = true;
            }
        }
        public static async Task InitOnLoaded() {
            if(!IsLoadedStageInitialized) {
                MediaPlayerElement.Instance.Init();
                StoreLocalPlayer.Instance.InitPlayerListeners();
                await StorePlayerManager.Instance.Player.TryToConnect();
                IsLoadedStageInitialized = true;
            }
        }
        // SuspensionManager is "internal", so apparently cannot be accessed
        // from another project
        public static void SuspensionManagerRegister(Frame frame, string sessionStateKey) {
            SuspensionManager.RegisterFrame(frame, "AppFrame");
        }
        public static async Task SuspensionManagerSave() {
            await SuspensionManager.SaveAsync();
        }
        public static async Task SuspensionManagerRestore() {
            try {
                await SuspensionManager.RestoreAsync();
            } catch(SuspensionManagerException) {
                //Something went wrong restoring state.
                //Assume there is no state and continue
            }
        }


    }
}
