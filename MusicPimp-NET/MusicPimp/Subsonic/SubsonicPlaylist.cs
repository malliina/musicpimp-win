using Mle.MusicPimp.Subsonic;
using Mle.MusicPimp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Audio {
    public class SubsonicPlaylist : BasePlaylist {
        private SubsonicSession session;

        public SubsonicPlaylist(SubsonicSession s) {
            session = s;
        }
        public override Task AddSong(MusicItem song) {
            var songId = int.Parse(song.Id);
            return session.serverAddToPlaylistAsync(songId);
        }
        protected override Task RemoveSongInternal(int playlistIndex) {
            return session.serverRemoveFromPlaylistAsync(playlistIndex);
        }
        protected override Task SendSkipCommand(int index) {
            return session.serverSkipToPlaylistIndexAsync(index);
        }
        public override Task SetPlaylist(MusicItem song) {
            var songId = int.Parse(song.Id);
            return session.serverSetPlaylistAsync(songId);
        }
        public override async Task LoadData() {
            var response = await session.serverGetPlaylistAsync();
            var playlistIndex = response.jukeboxPlaylist.currentIndex;
            // entry null => playlist empty
            var entries = response.jukeboxPlaylist.entry;
            if(entries != null) {
                var index = 0;
                var items = new List<PlaylistMusicItem>();
                foreach(var entry in entries) {
                    MusicItem musicItem = AudioConversions.EntryToMusicItem(entry, session.StreamUriFor(entry.id), session.Username, session.Password);
                    items.Add(new PlaylistMusicItem(musicItem, index++));
                }
                SyncPlaylist(items, playlistIndex);
            } else {
                SyncPlaylist(new List<PlaylistMusicItem>(), playlistIndex);
            }
        }
    }
}
