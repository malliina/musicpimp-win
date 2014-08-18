using Mle.MusicPimp.ViewModels;
using System.Collections.Generic;

namespace Mle.MusicPimp.Audio {
    public class MusicItemComparer : IEqualityComparer<MusicItem> {
        public bool Equals(MusicItem x, MusicItem y) {
            return x.Name == y.Name && x.Path == y.Path;
        }
        public int GetHashCode(MusicItem obj) {
            return obj.Name.GetHashCode() + obj.Path.GetHashCode();
        }
    }
}
