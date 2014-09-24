using Microsoft.Phone.Shell;
using Mle.Concurrent;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Util;
using Mle.MusicPimp.ViewModels;
using Mle.MusicPimp.Xaml;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace Mle.MusicPimp.Controls {
    public abstract class PimpBasePage : AsyncPhoneApplicationPage {
        protected abstract Task PrepareSelectedPivot();

        protected static class Pivots {
            public const int Music = 0;
            public const int Player = 1;
            public const int Playlist = 2;
            public const int Settings = 3;
        }
        protected const string SharedProjectName = "MusicPimp-WP8";


        public PimpViewModel AppModel {
            get { return PimpViewModel.Instance; }
        }
        protected MusicLibrary MusicProvider {
            get { return PhoneLibraryManager.Instance.MusicProvider; }
        }
        protected BasePlayer MusicPlayer {
            get { return PhonePlayerManager.Instance.Player; }
        }
        private PlayerManager playerManager {
            get { return PhonePlayerManager.Instance; }
        }
        private LibraryManager libraryManager {
            get { return PhoneLibraryManager.Instance; }
        }
        // player control
        protected ApplicationBarIconButton prevAppBarButton;
        protected ApplicationBarIconButton playAppBarButton;
        protected ApplicationBarIconButton pauseAppBarButton;
        protected ApplicationBarIconButton nextAppBarButton;
        protected ApplicationBarIconButton playlistAppBarButton;

        protected ApplicationBarMenuItem refreshMenuItem;

        protected List<ApplicationBarIconButton> currentButtons;

        protected AppBarHelper appBars;

        // hack to prevent multiple page instances from triggering xaml-specified callbacks
        protected static string latestPageId;
        protected string pageId;

        public PimpBasePage() {
            pageId = Guid.NewGuid().ToString();
            latestPageId = pageId;
            appBars = new AppBarHelper();
            InitializeAppBarButtons();
        }
        private void SetSeekability(BasePlayer player) {
            var isSkipAndSeekSupported = player.IsSkipAndSeekSupported;
            nextAppBarButton.IsEnabled = isSkipAndSeekSupported;
            prevAppBarButton.IsEnabled = isSkipAndSeekSupported;
        }
        protected override async void OnNavigatedTo(NavigationEventArgs args) {
            await HandleOpenAppFromLockScreenSettings();
            await OnNavigatedToAsync();
            SetSeekability(MusicPlayer);
            InstallPlayerStateEventHandler();
            libraryManager.ActiveEndpointChanged += libraryManager_ActiveEndpointChanged;
            playerManager.PlayerActivated += playerManager_PlayerActivated;
            playerManager.PlayerDeactivated += playerManager_PlayerDeactivated;
            // TODO document the reasoning for this condition; it has to do with fast app resume
            if(args.NavigationMode != NavigationMode.Reset) {
                base.OnNavigatedTo(args);
            }
        }
        /// <summary>
        /// The query string contains "WallpaperSettings" if the app is launched from the 
        /// phone's lock screen settings. In that case, this method sets a new lock screen 
        /// image. Note that the query string will be empty if the app is resumed from a
        /// suspended state, even if it's opened from the lock screen settings. So there's
        /// no guarantee that this will actually set the lock screen image immediately, 
        /// however if the user sets the app as the lock screen background provider, the
        /// background agent will eventually set an image the next time it's scheduled to 
        /// run.
        /// </summary>
        /// <returns></returns>
        private async Task HandleOpenAppFromLockScreenSettings() {
            var lockscreenKey = "WallpaperSettings";
            string lockscreenValue = "";
            bool lockscreenValueExists = NavigationContext.QueryString.TryGetValue(lockscreenKey, out lockscreenValue);
            if(lockscreenValueExists) {
                await LockScreenRequest.RequestThenSetLockScreen();
            }
        }

        protected virtual async Task OnNavigatedToAsync() {
            if(DataContext == null) {
                DataContext = AppModel;
            }
            await LifeCycleManager.Init();
            await LoadMusicItems();
            await PrepareSelectedPivot();
        }
        private Task LoadMusicItems() {
            // determines which folder to display
            string folder = ParseFolderId(NavigationContext);
            return AppModel.ParseAndLoad(folder);
        }
        /// <summary>
        /// Reads the query string.
        /// 
        /// Implementation note: the value of the 'folder' key, if any, is an encoded JSON object.
        /// </summary>
        /// <returns></returns>
        private string ParseFolderId(NavigationContext navContext) {
            string folder = "";
            navContext.QueryString.TryGetValue("folder", out folder);
            return folder;
        }
        private void libraryManager_ActiveEndpointChanged(MusicEndpoint e) {
            NavigationContext.QueryString.Clear();
        }

        private void playerManager_PlayerDeactivated(BasePlayer player) {
            player.IsPlayingChanged -= MusicPlayer_PlayerStateChanged;
            if(!player.IsEventBased) {
                PhoneNowPlaying.Instance.StopPolling();
            }
        }
        private async void playerManager_PlayerActivated(BasePlayer player) {
            player.IsPlayingChanged += MusicPlayer_PlayerStateChanged;

            SetSeekability(player);
            // asks server for status of new player
            if(!player.IsEventBased) {
                await player.UpdateNowPlaying();
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            libraryManager.ActiveEndpointChanged -= libraryManager_ActiveEndpointChanged;
            playerManager.PlayerActivated -= playerManager_PlayerActivated;
            playerManager.PlayerDeactivated -= playerManager_PlayerDeactivated;
            UninstallPlayerStateEventHandler();
            AppModel.NowPlayingModel.StopPollingIfNeeded();
            base.OnNavigatedFrom(e);
        }
        private void InstallPlayerStateEventHandler() {
            if(MusicPlayer != null) {
                MusicPlayer.IsPlayingChanged += MusicPlayer_PlayerStateChanged;
            }
        }
        private void UninstallPlayerStateEventHandler() {
            if(MusicPlayer != null) {
                MusicPlayer.IsPlayingChanged -= MusicPlayer_PlayerStateChanged;
            }
        }
        protected virtual void InitializeAppBarButtons() {
            prevAppBarButton = appBars.NewAppBarButton(appBars.assetHome + "appbar.transport.rew.rest.png", "previous", PrevApplicationBar_Click);
            playAppBarButton = appBars.NewAppBarButton(appBars.assetHome + "appbar.transport.play.rest.png", "play/pause", PlayPauseApplicationBar_Click);
            pauseAppBarButton = appBars.NewAppBarButton(appBars.assetHome + "appbar.transport.pause.rest.png", "pause", PlayPauseApplicationBar_Click);
            nextAppBarButton = appBars.NewAppBarButton(appBars.assetHome + "appbar.transport.ff.rest.png", "next", NextApplicationBar_Click);
            playlistAppBarButton = appBars.NewAppBarButton(appBars.assetHome + "appbar.cloud.download.png", "load", PlaylistApplicationBar_Click);
            refreshMenuItem = new ApplicationBarMenuItem("refresh");
            refreshMenuItem.Click += (sender, args) => { Refresh_Click(sender, args); };
        }
        protected List<ApplicationBarIconButton> PlayerButtons() {
            var playPauseButton = (MusicPlayer != null && MusicPlayer.IsPlaying) ? pauseAppBarButton : playAppBarButton;
            return new List<ApplicationBarIconButton>(new ApplicationBarIconButton[] { 
                prevAppBarButton, playPauseButton, nextAppBarButton 
            });
        }
        protected List<ApplicationBarIconButton> PlaylistButtons() {
            var buttons = PlayerButtons();
            buttons.Add(playlistAppBarButton);
            return buttons;
        }
        protected virtual List<ApplicationBarIconButton> SingleSelectButtons() {
            return PlayerButtons();
        }

        private void MusicPlayer_PlayerStateChanged(bool isPlaying) {
            if(isPlaying) {
                // replaces play with pause
                swap(playAppBarButton, pauseAppBarButton);
            } else {
                // replaces pause with play
                swap(pauseAppBarButton, playAppBarButton);
            }
        }
        /// <summary>
        /// Replaces one appbar button with another. 
        /// 
        /// The replacement takes the position (index) of the replaced button.
        /// </summary>
        /// <param name="victim"></param>
        /// <param name="replacement"></param>
        private void swap(ApplicationBarIconButton victim, ApplicationBarIconButton replacement) {
            var buttons = ApplicationBar.Buttons;
            // replace play with pause
            var victimIndex = buttons.IndexOf(victim);
            if(victimIndex >= 0) {
                buttons.RemoveAt(victimIndex);
                buttons.Insert(victimIndex, replacement);
            }
        }


        protected async void Refresh_Click(object sender, EventArgs e) {
            var wasUpdated = await EndpointScanner.Instance.SyncIfUnreachable(libraryManager.ActiveEndpoint);
            if(!wasUpdated) {
                // If the endpoint was updated, Reset is called anyway by PimpViewModel.
                // This ensures it's called exactly once.
                MusicProvider.Reset();
            }
            //await OnMusicItemsNavigatedTo();
            await LoadMusicItems();
        }
        /// <summary>
        /// clicking back does not fire OnPivotSelectionChanged, only OnNavigatedTo
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnMusicItemsNavigatedTo() {
            UpdateMusicLibraryAppBarButtons();
            return AsyncTasks.Noop();
        }
        protected virtual void UpdateMusicLibraryAppBarButtons() {
            appBars.SetAppBarButtons(SingleSelectButtons());
        }
        protected void SetDefaultAppBarButtons() {
            appBars.SetAppBarButtons(SingleSelectButtons());
        }

        protected void ManageRefreshMenuItem(int pivotIndex) {
            if(pivotIndex == Pivots.Music) {
                if(!ApplicationBar.MenuItems.Contains(refreshMenuItem)) {
                    ApplicationBar.MenuItems.Insert(0, refreshMenuItem);
                }
            } else {
                ApplicationBar.MenuItems.Remove(refreshMenuItem);
            }
        }
        // do not fucking drop s.w.i
        protected async void HandleMusicItemSelected(object sender, System.Windows.Input.GestureEventArgs e) {
            var musicItem = ((FrameworkElement)sender).DataContext as MusicItem;
            if(musicItem == null) {
                return;
            }
            await AppModel.OnSingleMusicItemSelected(musicItem);
        }
        protected virtual async Task OnPlaylistNavigatedTo() {
            appBars.SetAppBarButtons(PlaylistButtons());
            if(!MusicPlayer.IsEventBased) {
                await MusicPlayer.Playlist.Load();
            }
        }
        protected async Task OnPlayerNavigatedTo() {
            appBars.SetAppBarButtons(PlayerButtons());
            if(!MusicPlayer.IsEventBased) {
                await MusicPlayer.UpdateNowPlaying();
                AppModel.NowPlayingModel.StartPolling();
            }
        }
        // wtf?
        protected void PrevApplicationBar_Click(object sender, EventArgs e) {
            prevButton_Click(sender, null);
        }
        protected void NextApplicationBar_Click(object sender, EventArgs e) {
            nextButton_Click(sender, null);
        }
        protected void PlayPauseApplicationBar_Click(object sender, EventArgs e) {
            playButton_Click(sender, null);
        }
        protected void PlaylistApplicationBar_Click(object sender, EventArgs e) {
            GoToSharedPage(typeof(Playlists).Name);
        }
        protected async void playButton_Click(object sender, RoutedEventArgs e) {
            await MusicPlayer.OnPlayOrPause();
        }
        // if pressed when there's no prev (or next) track, a bad request is returned, which we don't care about 
        private async void prevButton_Click(object sender, RoutedEventArgs e) {
            await MusicPlayer.OnPrev();
        }
        private async void nextButton_Click(object sender, RoutedEventArgs e) {
            await MusicPlayer.OnNext();
        }
        protected void GoToSharedPage(string pageName) {
            GoToProjectPage(SharedProjectName, pageName);
        }
    }
}
