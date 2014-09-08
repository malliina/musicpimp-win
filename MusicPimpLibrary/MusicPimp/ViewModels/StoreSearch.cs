using Mle.MusicPimp.Audio;
using Mle.Xaml;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public class StoreSearch : SearchVM {
        public MusicActions Actions { get; private set; }
        public MusicItemsModel Model { get { return MusicItemsModel.Instance; } }
        public BasePlayer MusicPlayer { get { return Actions.MusicPlayer; } }
        public AppBarController AppBar { get { return Actions.AppBar; } }
        public StoreNowPlaying NowPlaying { get { return StoreNowPlaying.Instance; } }
        public StoreSearch() {
            Actions = new MusicActions();
        }
    }
}
