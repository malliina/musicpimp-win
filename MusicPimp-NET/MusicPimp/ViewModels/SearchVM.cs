using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Util;
using Mle.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mle.MusicPimp.ViewModels {
    public class SearchVM : ViewModelBase {
        protected LibraryManager LibraryManager {
            get { return ProviderService.Instance.LibraryManager; }
        }
        private MusicLibrary MusicProvider {
            get { return LibraryManager.MusicProvider; }
        }

        ObservableCollection<DataTrack> testResults = new ObservableCollection<DataTrack>(){
            new DataTrack("id1","artist1", "album1", "track1"),
            new DataTrack("id2","artist2", "album2", "track2")
        };
        private string term;
        public string Term {
            get { return term; }
            set { this.SetProperty(ref term, value); }
        }

        private ObservableCollection<DataTrack> searchResults;
        public ObservableCollection<DataTrack> SearchResults {
            get { return searchResults; }
            set { SetProperty(ref searchResults, value); }
        }
        public SearchVM() {
            SearchResults = new ObservableCollection<DataTrack>();
        }

        public async Task Search() {
            SearchResults.Clear();
            Debug.WriteLine("Searching: " + Term);
            var results = await MusicProvider.Search(Term);
            foreach(var result in results) {
                SearchResults.Add(result);
            }
        }
    }
}
