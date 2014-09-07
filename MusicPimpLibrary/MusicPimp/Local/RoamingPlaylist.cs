using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.ViewModels;
using Mle.Roaming.Network;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Local {
    public class RoamingPlaylist : LocalBasePlaylist {
        //private const string PlaylistKey = "playlist";
        private const string PlaylistKey = "HighPriority5";
        private const string PlaylistFile = "playlist.json";

        private RoamingSettings roamingSettings;

        public RoamingPlaylist() {
            roamingSettings = new RoamingSettings();
        }

        private Task SubmitDownload(MusicItem track) {
            return PimpStoreDownloader.Instance.SubmitDownload(track);
        }

        protected override async Task SendSkipCommand(int playlistIndex) {
            var info = await LoadPlaylist();
            info.CurrentIndex = playlistIndex;
            await SavePlaylist(info);
        }
        public override async Task AddSong(MusicItem song) {
            // TODO remove
            await SubmitDownload(song);
            var info = await LoadPlaylist();
            info.Tracks.Add(AudioConversions.ToPlaylistTrack(song));
            await SavePlaylist(info);
        }
        protected override async Task RemoveSongInternal(int playlistIndex) {
            Songs.RemoveAt(playlistIndex);
            var info = await LoadPlaylist();
            info.Tracks.RemoveAt(playlistIndex);
            if(playlistIndex == info.CurrentIndex) {
                info.CurrentIndex = NO_POSITION;
            } else if(playlistIndex < info.CurrentIndex && info.CurrentIndex > 0) {
                info.CurrentIndex -= 1;
            }
            Index = info.CurrentIndex;
            await SavePlaylist(info);
        }
        public override async Task SetPlaylist(MusicItem song) {
            // TODO remove
            await SubmitDownload(song);
            var newPlaylist = new PlaylistInfo(AudioConversions.ToPlaylistTrack(song));
            await SavePlaylist(newPlaylist);
        }
        public async Task<int> SkipNext() {
            var info = await LoadPlaylist();
            var newIndex = -1;
            if(info.Tracks.Count > info.CurrentIndex + 1) {
                info.CurrentIndex += 1;
                newIndex = info.CurrentIndex;
            }
            await SavePlaylist(info);
            return newIndex;
        }
        public async Task<int> SkipPrevious() {
            var info = await LoadPlaylist();
            var newIndex = 0;
            if(info.Tracks.Count > info.CurrentIndex - 1 && info.CurrentIndex > 0) {
                info.CurrentIndex -= 1;
                newIndex = info.CurrentIndex;
            }
            await SavePlaylist(info);
            return newIndex;
        }
        public Task SavePlaylist(PlaylistInfo info) {
            return roamingSettings.SaveToFile(PlaylistFile, info);
        }
        public override Task<PlaylistInfo> LoadPlaylist() {
            return roamingSettings.LoadFromFile<PlaylistInfo>(PlaylistFile, def: PlaylistInfo.Empty());
        }
    }
}
