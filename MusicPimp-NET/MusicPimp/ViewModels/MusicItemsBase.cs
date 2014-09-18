using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Network;
using Mle.Util;
using Mle.Xaml.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public abstract class MusicItemsBase : BaseViewModel {
        public event Action FolderLoaded;
        private FolderViewModel musicFolder = new FolderViewModel("", "");
        public FolderViewModel MusicFolder {
            get { return musicFolder; }
            set { this.SetProperty(ref this.musicFolder, value); }
        }
        private bool isLoadingFolder = false;
        public bool IsLoadingFolder {
            get { return isLoadingFolder; }
            protected set { SetProperty(ref isLoadingFolder, value); }
        }
        public abstract MusicLibrary MusicProvider { get; }
        public LocalMusicLibrary LocalLibrary { get; private set; }
        public abstract BasePlayer MusicPlayer { get; }
        public ICommand DisplayHelp { get; protected set; }
        public ICommand Refresh { get; protected set; }
        public ICommand PlayAllItems { get; private set; }
        public ICommand ToPlaylistAllItems { get; private set; }
        public ICommand MusicItemToPlaylistCommand { get; protected set; }
        public ICommand HandleMusicItemTap { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand DeleteLocally { get; private set; }
        public bool IsLibraryLocal {
            get { return MusicProvider == LocalLibrary; }
        }

        public static readonly string AppHelpHeader = "Connecting to a remote PC";
        public static readonly string AppHelpMessage = @"1. Download and install MusicPimp for your PC from www.musicpimp.org.
                        
2. Add folders containing MP3s in the web interface of MusicPimp for your PC.
                        
3. Add your PC as a music endpoint and set it as the music source in this app.
                        
4. Enjoy your music.";
        // the key is the folder id
        public IDictionary<string, MusicItem> LibraryScrollPositions { get; private set; }

        public MusicItemsBase(LocalMusicLibrary localLibrary) {
            musicFolder.IsLoading = true;
            LocalLibrary = localLibrary;
            DisplayHelp = new UnitCommand(() => Send(AppHelpHeader, AppHelpMessage));
            Refresh = new AsyncUnitCommand(RefreshCurrentFolder);

            PlayAllItems = new AsyncDelegateCommand<IList>(items => {
                return PlayAll(TypeHelpers.CollectionOf<MusicItem>(items));
            });
            ToPlaylistAllItems = new AsyncDelegateCommand<IList>(items => {
                return AddToPlaylistRecursively(TypeHelpers.CollectionOf<MusicItem>(items));
            });
            HandleMusicItemTap = new AsyncDelegateCommand<MusicItem>(item => {
                return OnSingleMusicItemSelected(item);
            });
            DeleteCommand = new DelegateCommand<MusicItem>(Delete);
            MusicItemToPlaylistCommand = new AsyncDelegateCommand<MusicItem>(AddToPlaylistRecursively);
            DeleteLocally = new AsyncDelegateCommand<MusicItem>(DeleteLocalItem);

            LibraryScrollPositions = new Dictionary<string, MusicItem>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>the current music item we should scroll to, or null</returns>
        public MusicItem CurrentScrollPosition() {
            var folderId = MusicFolder.FolderId;
            MusicItem item;
            var containsKey = LibraryScrollPositions.TryGetValue(folderId, out item);
            if(containsKey && item != null && MusicFolder.MusicItems.Contains(item)) {
                return item;
            } else {
                return null;
            }
        }
        private Task DeleteLocalItem(MusicItem item) {
            return TryFileDeletion(async () => {
                await LocalLibrary.Delete(item);
                // Updates the UI. Apparently, MusicFolder.MusicItems.Remove(item); does not update the UI.
                await RefreshCurrentFolder();
            });
        }
        protected void DeleteAll(IEnumerable<MusicItem> items) {
            foreach(var item in items) {
                Delete(item);
            }
        }
        protected void Delete(MusicItem song) {
            try {
                var shouldRemove = song.IsSourceLocal;
                LocalLibrary.Delete(song.Path);
                // refreshes the items in the current view
                if(shouldRemove) {
                    MusicFolder.MusicItems.Remove(song);
                }
            } catch(Exception e) {
                Send("Unable to delete. Perhaps the file is in use. " + e.Message);
            }
        }
        private async Task TryFileDeletion(Func<Task> code) {
            try {
                await code();
            } catch(Exception e) {
                Send("Unable to delete. Perhaps the file is in use. " + e.Message);
            }
        }
        public async Task<string> ParseAndLoad(string encodedFolder) {
            FolderMeta folder = ParseFolder(encodedFolder);
            return await LoadFolder(folder.Id, folder.Path);
        }
        public async Task ResetAndRefreshRoot() {
            MusicProvider.Reset();
            await ReloadRoot();
        }
        public async Task ReloadRoot() {
            await LoadFolder(MusicProvider.RootFolderKey, String.Empty);
        }
        public async Task RefreshCurrentFolder() {
            MusicProvider.Reset();
            await LoadFolder(MusicFolder.FolderId, MusicFolder.DisplayablePath);
        }
        /// <summary>
        /// Loads the specified folder unless it's cached.
        /// </summary>
        /// <param name="folder">the folder id</param>
        /// <returns>the feedback message</returns>
        public async Task<string> LoadFolder(string folder, string path) {
            if(MusicFolder != null) {
                MusicFolder.FolderLoaded -= MusicFolder_FolderLoaded;
            }
            MusicFolder = new FolderViewModel(folder, path);
            MusicFolder.FolderLoaded += MusicFolder_FolderLoaded;
            await MusicFolder.Load();
            return MusicFolder.FeedbackMessage;
        }
        protected virtual void MusicFolder_FolderLoaded() {
            OnFolderLoaded();
        }
        public FolderMeta ParseFolder(string encodedJson) {
            if(encodedJson == null || encodedJson == String.Empty) {
                return new FolderMeta(MusicProvider.RootFolderKey, String.Empty);
            } else {
                var json = Strings.decode(encodedJson);
                var coord = Json.Deserialize<FolderMeta>(json);
                return coord;
            }
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
        public Task AddToPlaylistRecursively(MusicItem item) {
            return AddToPlaylistRecursively(new List<MusicItem>() { item });
        }
        public async Task AddToPlaylistRecursively(IEnumerable<MusicItem> items) {
            await UsageControlled(async () => {
                var itemsList = items.ToList();
                var allTracks = new List<MusicItem>();
                foreach(var item in itemsList) {
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
        public async Task OnSingleMusicItemSelected(MusicItem item) {
            LibraryScrollPositions[MusicFolder.FolderId] = item;
            //MusicFolder.LatestSelection = item;
            if(item.IsDir) {
                string folderPath = MusicFolder.DisplayablePath == String.Empty ? item.Name : MusicFolder.DisplayablePath + "/" + item.Name;
                var folderIdentifier = MusicProvider.DirectoryIdentifier(item);
                var folderJson = Json.SerializeToString(new FolderMeta(folderIdentifier, folderPath));
                // encode the folder id so we can pass it in a uri query parameter
                var encodedFolderId = Strings.encode(folderJson);
                // user clicked a music directory
                Navigator.NavigateWithinSamePage(encodedFolderId);
            } else {
                // user clicked a song, let's play!
                await MusicPlayer.PlaySong(item);
            }
        }
        public async Task OnMusicSourceChanged() {
            await WithExceptionEvents(async () => {
                var feedback = await LoadFolder(MusicProvider.RootFolderKey, String.Empty);
                if(feedback == MusicProvider.RootEmptyMessage) {
                    Send("Library empty", MusicProvider.RootEmptyMessage);
                }
            });
        }
        private void OnFolderLoaded() {
            if(FolderLoaded != null) {
                FolderLoaded();
            }
        }
    }
    public class FolderMeta {
        public string Id { get; private set; }
        public string Path { get; private set; }
        public FolderMeta(string id, string path) {
            Id = id;
            Path = path;
        }
    }
}
