using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Mle.MusicPimp.ViewModels {
    public class PhoneNowPlaying : NowPlayingInfo {
        private static PhoneNowPlaying instance = null;
        public static PhoneNowPlaying Instance {
            get {
                if (instance == null)
                    instance = new PhoneNowPlaying();
                return instance;
            }
        }

        public override PlayerManager PlayerManager {
            get { return PhonePlayerManager.Instance; }
        }

        private static BitmapImage ImageFrom(string path) {
            return new BitmapImage(new Uri(path, UriKind.Relative));
        }
        private static string appBarImageDir = "/Assets/AppBar/";

        private static BitmapImage AppBarImage(string fileName) {
            return ImageFrom(appBarImageDir + fileName);
        }

        private BitmapImage muteImage = AppBarImage("appbar.sound.0.png");
        private BitmapImage lowVolumeImage = AppBarImage("appbar.sound.1.png");
        private BitmapImage mediumVolumeImage = AppBarImage("appbar.sound.2.png");
        private BitmapImage highVolumeImage = AppBarImage("appbar.sound.3.png");

        public ImageSource VolumeImage {
            get {
                if (Player.IsMute) {
                    return muteImage;
                } else {
                    if (Player.Volume < 30) {
                        return lowVolumeImage;
                    } else if (Player.Volume < 60) {
                        return mediumVolumeImage;
                    } else {
                        return highVolumeImage;
                    }
                }
            }
        }

        protected TickTimer updatePlayerTimer;

        protected PhoneNowPlaying() {
            updatePlayerTimer = new TickTimer(TimeSpan.FromMilliseconds(900));
            updatePlayerTimer.Timer.Tick += Timer_Tick;

            PlayerManager.ActiveEndpointChanged += e => InstallPlayerEventHandlers();
            if (Player != null) {
                InstallPlayerEventHandlers();
            } else {
                PlayerManager.Initialized += InstallPlayerEventHandlers;
            }
        }
        /// <summary>
        /// Listens for changes to the player state.
        /// 
        /// TODO: Consider callback lifetimes when the player object changes.
        /// </summary>
        private void InstallPlayerEventHandlers() {
            if (Player != null) {
                Player.VolumeChanged += newVolume => OnPropertyChanged("VolumeImage");
                Player.MuteToggled += muteOrNot => OnPropertyChanged("VolumeImage");
            }
        }
        public override void StartPolling() {
            updatePlayerTimer.Start();
        }
        public override void StopPolling() {
            updatePlayerTimer.Stop();
        }
    }
}
