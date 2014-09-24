
namespace Mle.MusicPimp.Audio {
    public class SavedPlaylistMeta {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public int SongCount { get; private set; }
        public SavedPlaylistMeta(string id, string name, int songCount) {
            Id = id;
            Name = name;
            SongCount = songCount;
        }
        public static SavedPlaylistMeta Empty() {
            return new SavedPlaylistMeta("0", "nonexistent", 0);
        }
    }
}
