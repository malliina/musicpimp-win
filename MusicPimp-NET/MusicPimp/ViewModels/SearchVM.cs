using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Util;
using Mle.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        }

        public async Task Search() {
            SearchResults.Clear();
            Debug.WriteLine("Searching: " + Term);
            var results = await WebAwareT(async () => {
                return await MusicProvider.Search(Term);
            });
            foreach(var result in results) {
                SearchResults.Add(result);
            }
        }
    }
}
