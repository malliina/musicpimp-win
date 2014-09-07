using Mle.MusicPimp.Audio;
using Mle.MusicPimp.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Local {
    public abstract class LocalBasePlaylist : BasePlaylist {
        public abstract Task<PlaylistInfo> LoadPlaylist();
        public override async Task LoadData() {
            // might return null but I don't know why
            var playlistInfo = await LoadPlaylist();
            if(playlistInfo != null) {
                var tracks = playlistInfo.Tracks;
                List<MusicItem> items = playlistInfo.Tracks
                    .Select(i => AudioConversions.playTrack2musicItem(i, null, null))
                    .ToList();
                Sync(items, playlistInfo.CurrentIndex);
            }
        }
    }
}
