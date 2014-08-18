using Mle.MusicPimp.Network;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Beam {
    public abstract class PersistentBeamPlaylist : BeamPlaylist {

        protected readonly string fileName;

        public PersistentBeamPlaylist(PimpSession session, PimpWebSocket webSocket, BeamPlayer player)
            : base(session, webSocket, player) {
            fileName = session.Username + ".json";
            webSocket.Welcomed += webSocket_Welcomed;
            webSocket.Disconnected += PlayerDisconnected;
        }

        private async void webSocket_Welcomed() {
            SyncPlaylist(await LoadItems(fileName));
        }

        protected virtual void PlayerDisconnected(string user) {
            if (session.Username == user) {
                Songs.Clear();
                SyncPlaylist(Songs);
                Delete(fileName);
            }
        }
        protected abstract Task Delete(string fileName);
        protected abstract Task<ObservableCollection<PlaylistMusicItem>> LoadItems(string filePath);
        protected abstract Task Persist(ObservableCollection<PlaylistMusicItem> items);

        protected override async Task Add(MusicItem song) {
            await base.Add(song);
            await Persist(Songs);
        }
    }
}
