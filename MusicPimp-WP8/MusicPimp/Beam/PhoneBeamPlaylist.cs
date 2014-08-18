using Mle.Concurrent;
using Mle.IO;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using Mle.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Beam {
    public class PhoneBeamPlaylist : PersistentBeamPlaylist {
        public PhoneBeamPlaylist(PimpSession session, PimpWebSocket webSocket, BeamPlayer player)
            : base(session, webSocket, player) {
        }
        private JsonUtils Json { get { return JsonUtils.Instance; } }

        protected override Task<ObservableCollection<PlaylistMusicItem>> LoadItems(string filePath) {
            return TaskEx.Run(() => {
                return Json.DeserializeFileOrElse(filePath, new ObservableCollection<PlaylistMusicItem>());
            });
        }
        protected override Task Persist(ObservableCollection<PlaylistMusicItem> items) {
            return Json.SerializeToFile(items, fileName);
        }
        protected override Task Delete(string fileName) {
            FileUtils.Delete(fileName);
            return AsyncTasks.Noop();
        }
    }
}
