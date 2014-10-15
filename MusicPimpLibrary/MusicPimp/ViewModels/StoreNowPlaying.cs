using Mle.Xaml;
using Mle.Xaml.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Mle.MusicPimp.ViewModels {
    public class StoreNowPlaying : NowPlayingInfo {

        private static StoreNowPlaying instance = null;
        public static StoreNowPlaying Instance {
            get {
                if (instance == null)
                    instance = new StoreNowPlaying();
                return instance;
            }
        }
        public override PlayerManager PlayerManager {
            get { return StorePlayerManager.Instance; }
        }
        public AppBarController AppBar { get; private set; }

        protected DispatcherTimer Timer { get; private set; }

        private ObservableCollection<PlaylistMusicItem> selected;
        public ObservableCollection<PlaylistMusicItem> Selected {
            get { return selected; }
            set { SetProperty(ref selected, value); }
        }
        public List<PlaylistMusicItem> SelectedList { get { return Selected.ToList(); } }
        public Style VolumeButtonStyle {
            get {
                var styleName = Player.IsMute ?
                    "MuteAppBarButtonNoTextStyle" : "VolumeAppBarButtonNoTextStyle";
                return Application.Current.Resources[styleName] as Style;
            }
        }
        public Style LightPlayPauseButtonStyle {
            get {
                var styleName = Player.IsPlaying ?
                    "LightPauseAppBarButtonStyle" : "LightPlayAppBarButtonStyle";
                return Application.Current.Resources[styleName] as Style;
            }
        }
        public Style PlayPauseButtonStyle {
            get {
                var styleName = Player.IsPlaying ?
                    "PauseAppBarButtonStyle" : "PlayAppBarButtonStyle";
                return Application.Current.Resources[styleName] as Style;
            }
        }

        public ICommand RemoveSelected { get; private set; }

        protected StoreNowPlaying() {
            AppBar = new AppBarController();
            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromMilliseconds(900);
            Timer.Tick += Timer_Tick;
            Selected = new ObservableCollection<PlaylistMusicItem>();
            Selected.CollectionChanged += (s, e) => AppBar.Update(SelectedList);
            RemoveSelected = new AsyncUnitCommand(() => {
                return Player.Playlist.RemoveSong(SelectedList.First().Index);
            });
            // TODO: event handler life cycles
            PlayerManager.ActiveEndpointChanged += e => InstallPlayerEventHandlers();
            InstallPlayerEventHandlers();
        }

        private void InstallPlayerEventHandlers() {
            var player = PlayerManager.Player;
            if (player != null) {
                player.IsPlayingChanged += s => {
                    OnPropertyChanged("PlayPauseButtonStyle");
                    OnPropertyChanged("LightPlayPauseButtonStyle");
                };
                player.MuteToggled += muteOrNoMute => OnPropertyChanged("VolumeButtonStyle");
            }
        }

        public override void StartPolling() {
            if (!Timer.IsEnabled) {
                Timer.Start();
            }
        }
        public override void StopPolling() {
            if (Timer.IsEnabled) {
                Timer.Stop();
            }
        }
    }
}
