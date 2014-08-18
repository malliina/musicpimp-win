using Mle.MusicPimp.Network;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using Mle.Roaming.Network;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;

namespace Mle.MusicPimp.Beam {
    public class StoreBeamPlaylist : PersistentBeamPlaylist {

        private RoamingSettings roaming;

        public StoreBeamPlaylist(PimpSession session, PimpWebSocket webSocket, BeamPlayer player)
            : base(session, webSocket, player) {
            roaming = new RoamingSettings();
        }

        protected override async Task Delete(string fileName) {
            var file = await roaming.RoamingFolder.GetFileAsync(fileName);
            await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        protected override Task<ObservableCollection<PlaylistMusicItem>> LoadItems(string filePath) {
            return roaming.LoadFromFile<ObservableCollection<PlaylistMusicItem>>(filePath,
                def: new ObservableCollection<PlaylistMusicItem>());
        }

        protected override Task Persist(ObservableCollection<PlaylistMusicItem> items) {
            return roaming.SaveToFile(fileName, items);
        }
    }
}
