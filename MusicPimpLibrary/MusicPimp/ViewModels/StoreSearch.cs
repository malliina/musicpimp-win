using Mle.MusicPimp.Audio;
using Mle.Xaml;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public class StoreSearch : SearchVM {
        public MusicItemsModel Model { get { return MusicItemsModel.Instance; } }
        public BasePlayer MusicPlayer { get { return Model.MusicPlayer; } }
        public AppBarController AppBar { get { return Model.AppBar; } }
        public StoreNowPlaying NowPlaying { get { return StoreNowPlaying.Instance; } }
        public ICommand HandleMusicItemTap { get { return Model.HandleMusicItemTap; } }
        public ICommand PlaySelected { get { return Model.PlaySelected; } }
        public ICommand AddSelected { get { return Model.AddSelected; } }
        public ICommand DeleteSelected { get { return Model.DeleteSelected; } }
        public ICommand DownloadSelected { get { return Model.DownloadSelected; } }
    }
}
