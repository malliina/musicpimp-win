using Mle.MusicPimp.Audio;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mle.MusicPimp.ViewModels {
    public class PlaylistVM : PlaylistBase {
        private IList<MusicItem> tracks;
        public IList<MusicItem> Tracks {
            get { return tracks; }
            set { SetProperty(ref tracks, value); }
        }
        public SavedPlaylistMeta Meta { get; private set; }
        public PlaylistVM(SavedPlaylistMeta meta) {
            Meta = meta;
        }
        public override async Task Update() {
            await WebAware(async () => {
                Tracks = await Library.LoadPlaylist(Meta.Id);
            });
            if(FeedbackMessage == null && Tracks.Count == 0) {
                FeedbackMessage = "no tracks";
            }
        }
        public Task DeleteCurrent() {
            return WithExceptionEvents(async () => await Library.DeletePlaylist(Meta.Id));
        }
        public Task PlayCurrent() {
            return PlayPlaylist(Meta);
        }
    }
}
