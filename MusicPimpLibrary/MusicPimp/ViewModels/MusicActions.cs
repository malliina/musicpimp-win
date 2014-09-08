using Mle.IO;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Network;
using Mle.Xaml;
using Mle.Xaml.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public class MusicActions : BaseViewModel {
        //public event Action<MusicItem> ItemDeleted;
        public BasePlayer MusicPlayer { get { return StorePlayerManager.Instance.Player; } }
        public MusicLibrary MusicProvider { get { return StoreLibraryManager.Instance.MusicProvider; } }
        public AppBarController AppBar { get; private set; }
        private ObservableCollection<MusicItem> selected;
        public ObservableCollection<MusicItem> Selected {
            get { return selected; }
            set { SetProperty(ref selected, value); }
        }
        public ICommand HandleMusicItemTap { get; private set; }
        public ICommand PlaySelected { get; private set; }
        public ICommand AddSelected { get; private set; }
        //public ICommand DeleteSelected { get; private set; }
        public ICommand DownloadSelected { get; private set; }

        public bool CanDeleteSelection {
            // true if each selection is a track in app local storage
            get { return Selected.Count > 0 && !Selected.Any(i => i.IsDir || !i.IsSourceLocal || FileUtilsBase.IsLocalNonAppFile(i.Source)); }
        }

        public MusicActions() {
            AppBar = new AppBarController();
            Selected = new ObservableCollection<MusicItem>();
            Selected.CollectionChanged += (s, e) => SelectionChanged();
            //DeleteSelected = new UnitCommand(() => DeleteAll(Selected));
            PlaySelected = new AsyncUnitCommand(() => PlayAll(Selected));
            AddSelected = new AsyncUnitCommand(() => AddToPlaylistRecursively(Selected));
            DownloadSelected = new AsyncUnitCommand(() => PimpStoreDownloader.Instance.SubmitAll(Selected));
            HandleMusicItemTap = new AsyncDelegateCommand<MusicItem>(item => {
                return OnSingleMusicItemSelected(item);
            });
        }
        public void ClearSelection() {
            Selected.Clear();
        }
        public async Task OnSingleMusicItemSelected(MusicItem item) {
            await MusicPlayer.PlaySong(item);
        }
        private void SelectionChanged() {
            AppBar.Update(Selected);
            OnPropertyChanged("CanDeleteSelection");
        }
        //protected void DeleteAll(IEnumerable<MusicItem> items) {
        //    foreach(var item in items) {
        //        Delete(item);
        //    }
        //}
        //protected void Delete(MusicItem song) {
        //    try {
        //        var shouldRemove = song.IsSourceLocal;
        //        LocalLibrary.Delete(song.Path);
        //        // refreshes the items in the current view
        //        if(shouldRemove) {
        //            MusicFolder.MusicItems.Remove(song);
        //        }
        //    } catch(Exception e) {
        //        Send("Unable to delete. Perhaps the file is in use. " + e.Message);
        //    }
        //}
        public async Task AddToPlaylistRecursively(IEnumerable<MusicItem> items) {
            await UsageControlled(async () => {
                var allTracks = new List<MusicItem>();
                foreach(var item in items) {
                    var tracks = await GetSongsRecursively(item);
                    allTracks.AddRange(tracks);
                }
                await MusicPlayer.Playlist.AddSongs(allTracks);
            });
        }
        public async Task PlayAll(IEnumerable<MusicItem> items) {
            await WithExceptionEvents(async () => {
                var tracks = new List<MusicItem>();
                // traverses directories recursively and obtains all songs
                foreach(var item in items.ToList()) {
                    var itemTracks = await GetSongsRecursively((MusicItem)item);
                    tracks.AddRange(itemTracks);
                }
                await MusicPlayer.PlayPlaylist(tracks);
            });
        }
        public async Task<List<MusicItem>> GetSongsRecursively(MusicItem songOrDir) {
            if(!songOrDir.IsDir) {
                var tracks = new List<MusicItem>();
                tracks.Add(songOrDir);
                return tracks;
            } else {
                return await MusicProvider.SongsInFolder(songOrDir);
            }
        }
        //private void OnItemDeleted(MusicItem item) {
        //    if(ItemDeleted != null) {
        //        ItemDeleted(item);
        //    }
        //}
    }
}
