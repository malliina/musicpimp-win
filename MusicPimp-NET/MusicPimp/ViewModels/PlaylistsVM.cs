using Mle.Messaging;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Messaging;
using Mle.MusicPimp.Util;
using Mle.ViewModels;
using Mle.Xaml.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public class PlaylistsVM : WebAwareLoading {
        private IEnumerable<SavedPlaylistMeta> playlists;
        public IEnumerable<SavedPlaylistMeta> Playlists {
            get { return playlists; }
            set { SetProperty(ref playlists, value); }
        }
        public ICommand NavigateToPlaylist { get; private set; }
        public ICommand Play { get; private set; }
        public ICommand Delete { get; private set; }
        public ProviderService Provider { get { return ProviderService.Instance; } }
        public BasePlayer Player { get { return Provider.PlayerManager.Player; } }
        public MusicLibrary Library { get { return Provider.LibraryManager.MusicProvider; } }
        public PlaylistsVM() {
            Playlists = new List<SavedPlaylistMeta>();
            NavigateToPlaylist = new DelegateCommand<SavedPlaylistMeta>(playlist => {
                var queryParams = new Dictionary<string, string>();
                queryParams.Add(PageParams.ID, playlist.Id);
                PageNavigationService.Instance.NavigateWithQuery(PageNames.PLAYLIST, queryParams);
            });
            Play = new AsyncDelegateCommand<SavedPlaylistMeta>(PlayPlaylist);
            Delete = new AsyncDelegateCommand<SavedPlaylistMeta>(playlist => Library.DeletePlaylist(playlist.Id));
        }
        public async Task PlayPlaylist(SavedPlaylistMeta playlist) {
            var tracks = await Library.LoadPlaylist(playlist.Id);
            await Player.PlayPlaylist(tracks);
        }
        public Task Update() {
            return WebAware(async () => {
                Playlists = await Library.LoadPlaylists();
            });
        }
    }
}
