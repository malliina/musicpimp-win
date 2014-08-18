using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Database {
    public abstract class PlaybackHistorianBase {
        public abstract Task AddPlayCount(string songPath);
        public abstract Task DeleteSongEntry(string songPath);
        public abstract Task<List<string>> LeastPlayed(int count = 20);
        public async Task<List<string>> PopLeastPlayed(int count = 20) {
            var songs = await LeastPlayed(count);
            foreach (var song in songs) {
                await DeleteSongEntry(song);
            }
            return songs;
        }
    }
}
