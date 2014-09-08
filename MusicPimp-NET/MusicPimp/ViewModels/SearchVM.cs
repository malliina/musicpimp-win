using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Util;
using Mle.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public class SearchVM : WebAwareLoading {
        public ProviderService Provider {
            get { return ProviderService.Instance; }
        }
        protected LibraryManager LibraryManager {
            get { return Provider.LibraryManager; }
        }
        private MusicLibrary MusicProvider {
            get { return LibraryManager.MusicProvider; }
        }
        public ICommand AddToPlaylist {
            get { return Provider.PlayerManager.Player.Playlist.AddToPlaylistCommand; }
        }
        public ICommand Download { get { return Provider.Downloader.Download; } }
        public bool IsLibraryLocal { get { return Provider.MusicItemsBase.IsLibraryLocal; } }
        public bool IsLibraryOnline { get { return MusicProvider.IsOnline; } }

        public string LibraryStatus {
            get {
                if(IsLibraryLocal) {
                    return "To enable search, please set the music source to your MusicPimp or Subsonic server.";
                } else if(!IsLibraryOnline) {
                    return "Unable to connect to the music server.";
                } else {
                    return null;
                }
            }
        }

        private string term;
        public string Term {
            get { return term; }
            set { this.SetProperty(ref term, value); }
        }
        private ObservableCollection<MusicItem> searchResults;
        public ObservableCollection<MusicItem> SearchResults {
            get { return searchResults; }
            set { SetProperty(ref searchResults, value); }
        }
        public SearchVM() {
            SearchResults = new ObservableCollection<MusicItem>();
            UpdateStatus();
        }
        public void UpdateStatus() {
            FeedbackMessage = LibraryStatus;
        }

        public Task Search(string term) {
            Term = term;
            return Search();
        }
        public async Task Search() {
            SearchResults.Clear();
            var status = LibraryStatus;
            if(status == null) {
                //Debug.WriteLine("Searching: " + Term);
                // Search cancellation is not directly supported. Instead, when a search is complete, before updating the search results we ensure that the search 
                // term has not changed. If the search term has changed meanwhile, it means we've received out-of-date data, in which case it is ignored.
                var searchTerm = Term;
                var results = await WebAwareT(async () => {
                    return await MusicProvider.Search(Term);
                });
                // WebAwareT returns default(T) if there's an exception
                var isResultUpToDate = searchTerm == Term;
                if(results != null && isResultUpToDate) {
                    foreach(var result in results) {
                        SearchResults.Add(result);
                    }
                }
            } else {
                FeedbackMessage = status;
            }
        }
    }
}
