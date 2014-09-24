using Mle.Messaging;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Messaging;
using Mle.MusicPimp.Util;
using Mle.ViewModels;
using Mle.Xaml.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using Mle.Util;

namespace Mle.MusicPimp.ViewModels {
    public class PlaylistsVM : PlaylistBase {
        private IList<SavedPlaylistMeta> playlists;
        public IList<SavedPlaylistMeta> Playlists {
            get { return playlists; }
            set { SetProperty(ref playlists, value); }
        }
        public ICommand NavigateToPlaylist { get; private set; }
        public PlaylistsVM() {
            Playlists = new List<SavedPlaylistMeta>();
            NavigateToPlaylist = new DelegateCommand<SavedPlaylistMeta>(playlist => {
                var queryParams = new Dictionary<string, string>();
                var encodedJson = Strings.encode(Json.SerializeToString(playlist));
                queryParams.Add(PageParams.META, encodedJson);
                PageNavigationService.Instance.NavigateWithQuery(PageNames.PLAYLIST, queryParams);
            });
        }
        public override async Task Update() {
            await WebAware(async () => {
                Playlists = await Library.LoadPlaylists();
            });

            if(FeedbackMessage == null && Playlists.Count == 0) {
                FeedbackMessage = "no saved playlists";
            }
        }
    }
}
