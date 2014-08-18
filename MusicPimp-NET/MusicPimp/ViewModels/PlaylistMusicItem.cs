
using Mle.ViewModels;

namespace Mle.MusicPimp.ViewModels {
    public class PlaylistMusicItem : ViewModelBase {
        public MusicItem Song { get; private set; }
        private int index;
        public int Index {
            get { return index; }
            set { this.SetProperty(ref this.index, value); }
        }

        public PlaylistMusicItem(MusicItem song, int index) {
            Song = song;
            Index = index;
        }
        private bool isSelected;
        public bool IsSelected {
            get { return isSelected; }
            set { this.SetProperty(ref this.isSelected, value); }
        }
    }
}
