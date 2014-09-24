using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Util;
using Mle.ViewModels;
using Mle.Xaml.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public abstract class PlaylistBase : WebAwareLoading {
        public ProviderService Provider { get { return ProviderService.Instance; } }
        public BasePlayer Player { get { return Provider.PlayerManager.Player; } }
        public MusicLibrary Library { get { return Provider.LibraryManager.MusicProvider; } }
        public ICommand Play { get; private set; }
        public ICommand Delete { get; private set; }
        public PlaylistBase() {
            Play = new AsyncDelegateCommand<SavedPlaylistMeta>(PlayPlaylist);
            Delete = new AsyncDelegateCommand<SavedPlaylistMeta>(DeletePlaylistAndReload);
        }
        public async Task PlayPlaylist(SavedPlaylistMeta playlist) {
            await WithExceptionEvents(async () => {
                var tracks = await Library.LoadPlaylist(playlist.Id);
                await Player.PlayPlaylist(tracks);
            });
        }
        public async Task DeletePlaylistAndReload(SavedPlaylistMeta playlist) {
            await WithExceptionEvents(async () => {
                await Library.DeletePlaylist(playlist.Id);
            });
            await Update();
        }
        public abstract Task Update();
    }
}
