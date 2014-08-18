using Mle.MusicPimp.Database;
using Mle.ViewModels;
using System.Collections.ObjectModel;

namespace Mle.MusicPimp.ViewModels {
    public class MostPlayedViewModel : ViewModelBase {
        private ObservableCollection<PlayFrequency> mostPlayed;
        public ObservableCollection<PlayFrequency> MostPlayed {
            get { return mostPlayed; }
            set { this.SetProperty(ref this.mostPlayed, value); }
        }
        public MostPlayedViewModel() {
            MostPlayed = PlaybackHistorian.MostPlayed();
        }
    }
}
