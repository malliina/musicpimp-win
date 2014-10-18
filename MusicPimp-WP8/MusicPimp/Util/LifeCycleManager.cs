using Microsoft.Phone.Net.NetworkInformation;
//#define DEBUG_AGENT
using Microsoft.Phone.Scheduler;
using Mle.IO;
using Mle.Messaging;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Database;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Tiles;
using Mle.MusicPimp.ViewModels;
using Mle.Network;
using Mle.Util;
using Mle.ViewModels;
using System;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Windows;

namespace Mle.MusicPimp.Util {
    /// <summary>
    /// Common code for App.xaml.cs because we may have many separate App.xaml.cs files (one for WP7, another for WP8, ...).
    /// </summary>
    public class LifeCycleManager {
        private static bool IsAppInitialized = false;
        public volatile static bool AppWasActivated = false;

        private const string BackgroundTaskName = "TileUpdater";

        private static BasePlayer Player {
            get { return PhonePlayerManager.Instance.Player; }
        }
        private static void SendMessage(string msg) {
            MessagingService.Instance.Send(msg);
        }
        /// <summary>
        /// Idempotently initializes the application.
        /// 
        /// Call from an OnNavigatedTo method in order to ensure the UI is ready to receive any error messages.
        /// 
        /// Do not call from App.xaml.cs.
        /// </summary>
        /// <returns></returns>
        public static async Task Init() {
            if(!IsAppInitialized) {
                await TryInitTiles();
                var t = TryMaintainCacheLimit();
                var t2 = Utils.SuppressAsync<Exception>(Player.TryToConnect);
                TryInitBackgroundTask();
                InstallNetworkChangeListener();
                IsAppInitialized = true;
            }
            if(AppWasActivated) {
                AppWasActivated = false;
                var t3 = Player.TryToConnect();
            }
        }
        private static void InstallNetworkChangeListener() {
            DeviceNetworkInformation.NetworkAvailabilityChanged += async (s, e) => {
                if(e.NotificationType == NetworkNotificationType.InterfaceConnected
                    && DeviceNetworkInformation.IsWiFiEnabled) {
                    await Utils.SuppressAsync<Exception>(EndpointScanner.Instance.SyncActiveEndpoints);
                }
            };
        }

        private static async Task TryInitTiles() {
            try {
                await Tiles.Tiles.Instance.Init();
            } catch(Exception) {
            }
        }
        private static async Task TryMaintainCacheLimit() {
            try {
                await PhoneLocalLibrary.Instance.MaintainCacheLimit();
            } catch(IsolatedStorageException) {
                // file may be in use or some such
            } catch(Exception e) {
                // hmm
                SendMessage("Unable to maintain cache limit: " + e.Message);
            }
        }
        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh202942(v=vs.105).aspx
        /// 
        /// </summary>
        private static void TryInitBackgroundTask() {
            try {
                var task = ScheduledActionService.Find(BackgroundTaskName) as PeriodicTask;
                var found = task != null;
                if(!found) {
                    task = new PeriodicTask(BackgroundTaskName);
                }
                // shown in WP8 under Settings -> applications -> background tasks -> MusicPimp
                task.Description = "Updates the live tiles with album covers if available. Also updates the lock screen background if enabled.";
                task.ExpirationTime = DateTime.Now.AddDays(14);
                try {
                    if(!found) {
                        ScheduledActionService.Add(task);
                    } else {
                        ScheduledActionService.Remove(BackgroundTaskName);
                        ScheduledActionService.Add(task);
                    }
                } catch(InvalidOperationException) {
                    /// If you attempt to add a periodic background agent when the device’s limit has been exceeded, 
                    /// the call to Add(ScheduledAction) will throw an InvalidOperationException. 
                    /// 
                    /// If agents have been disabled for your app, attempts to register an agent using the 
                    /// Add(ScheduledAction) method of the ScheduledActionService will throw an InvalidOperationException.
                } catch(SchedulerServiceException) {
                    /// A SchedulerServiceException can also be thrown when adding a task, for example, 
                    /// if the device has just booted and the Scheduled Action Service hasn’t started yet.
                }
            } catch(Exception) { }


            //#if DEBUG_AGENT
            //                ScheduledActionService.LaunchForTest(BackgroundTaskName, TimeSpan.FromSeconds(10));
            //#endif
        }

        /// <summary>
        /// Initializes failsafe parts of the app.
        /// 
        /// Safe to call from App.xaml.cs constructor.
        /// </summary>
        public static void Start() {
            ProviderService.Instance.Register(new PhoneProvider());
            Application.Current.Host.Settings.EnableFrameRateCounter = false;
            FileUtils.CreateDirIfNotExists(PhoneLocalLibrary.Instance.BaseMusicPath);
            FileUtils.CreateDirIfNotExists(PhoneCoverService.CoverFolder);
            // initializes the database if it doesn't exist
            MusicDataContext.CreateIfNotExists();
            DownloadDataContext.CreateIfNotExists();
            UiService.Instance.SetUiUtils(PhoneUtil.Instance);
        }

        public static void Deactivate() {
            try {
                Player.Unsubscribe();
            } catch(Exception) { }
        }
    }
}
