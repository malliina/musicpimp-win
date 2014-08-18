using Mle.MusicPimp.Audio;
using Mle.ViewModels;
using System.Collections.ObjectModel;

namespace Mle.MusicPimp.ViewModels {
    public class SplashModel : Navigable {
        public ObservableCollection<TitledImageItem> SplashButtons { get; private set; }
        public BasePlayer MusicPlayer {
            get { return MusicItemsModel.Instance.MusicPlayer; }
        }
        public NowPlayingInfo NowPlaying {
            get { return StoreNowPlaying.Instance; }
        }

        public SplashModel() {
            SplashButtons = new ObservableCollection<TitledImageItem>() {
                FolderItem, PlayerItem, BeamItem, SettingsItem//,TestItem
            };
        }
    }
}
