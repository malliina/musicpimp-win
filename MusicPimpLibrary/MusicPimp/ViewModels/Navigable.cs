using Mle.Messaging;
using Mle.MusicPimp.Tiles;
using Mle.MusicPimp.Xaml;
using Mle.ViewModels;
using Mle.Xaml;
using Mle.Xaml.Commands;
using System.Windows.Input;
using Mle.Collections;

namespace Mle.MusicPimp.ViewModels {
    public class Navigable : ViewModelBase {

        public TitledImageItem HomeItem { get; private set; }
        public TitledImageItem FolderItem { get; private set; }
        public TitledImageItem SmallFolderItem { get; private set; }
        public TitledImageItem PlayerItem { get; private set; }
        public TitledImageItem SmallPlayerItem { get; private set; }
        public TitledImageItem SettingsItem { get; private set; }
        public TitledImageItem DownloadsItem { get; private set; }
        public TitledImageItem BeamItem { get; private set; }
        public TitledImageItem TestItem { get; private set; }

        public ICommand GoToSplash { get; private set; }
        public ICommand GoToLibrary { get; private set; }
        public ICommand GoToPlayer { get; private set; }
        public ICommand GoToDownloads { get; private set; }
        public ICommand GoToBeam { get; private set; }
        public ICommand GoToLicense { get; private set; }

        public Navigable() {
            string assetHome = "ms-appx:///MusicPimpLibrary/Assets/";
            HomeItem = new TitledImageItem(assetHome + "home-brown-48.png", "Home", "Go home", onClicked: GoTo<SplashPage>);
            FolderItem = new TitledImageItem(assetHome + "folder-open-foldercolor-1024.png", "Music", "Browse the library", onClicked: GoTo<MusicItems>);
            SmallFolderItem = new TitledImageItem(assetHome + "folder-open-ffcc66-48.png", "Music", "Browse the library", onClicked: GoTo<MusicItems>);
            PlayerItem = new TitledImageItem(assetHome + "music-green-128.png", "Player & playlist", "Control playback", onClicked: GoTo<Player>);
            SmallPlayerItem = new TitledImageItem(assetHome + "music-green-48.png", "Player & playlist", "Control playback", onClicked: GoTo<Player>);
            SettingsItem = new TitledImageItem(assetHome + "wrench-gray-1024.png", "Settings", "Manage libraries and playback", onClicked: OpenSettings);
            DownloadsItem = new TitledImageItem(assetHome + "download-alt-lightgreen-128.png", "Downloads", "In the background", onClicked: GoTo<Downloads>);
            BeamItem = new TitledImageItem(assetHome + "upload-alt-blue-128.png", "MusicBeamer", "Stream music to another PC", onClicked: GoTo<BarcodePage>);
            TestItem = new TitledImageItem(assetHome + "play-green.png", "Test", "Run a test", onClicked: Test);

            GoToLibrary = CommandTo<MusicItems>();
            GoToPlayer = CommandTo<Player>();
            GoToDownloads = CommandTo<Downloads>();
            GoToSplash = CommandTo<SplashPage>();
            GoToBeam = CommandTo<BarcodePage>();
            GoToLicense = CommandTo<IapPage>();
        }
        private async void Test() {
            var covers = await StoreCoverService.Instance.GetCoverCollection();
            if(covers.Count > 0) {
                RootPageViewModel.Instance.SetBackgroundUri(covers.Head());
            } else {
                //var tmp = 0;
            }
        }

        private void OpenSettings() {
            FlyoutManager.OpenFlyout<Settings>("Options");
        }

        private ICommand CommandTo<TPageType>() {
            return new UnitCommand(GoTo<TPageType>);
        }

        protected void GoTo<TPageType>() {
            PageNavigationService.Instance.NavigateToPage(typeof(TPageType));
        }
    }
}
