using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mle.Phone.Xaml.Controls {
    public class Grouping {
        //const string TracksGroupKey = "\uD83C\uDF10";
        public static readonly string DigitGroupHeader = "#";
        public static readonly string SongGroupHeader = ".";
        private List<string> headers = new List<string>((DigitGroupHeader + "abcdefghijklmnopqrstuvwxyz" + SongGroupHeader).ToCharArray().Select(c => c.ToString()));

        public ReadOnlyCollection<string> GroupDisplayNames { get; private set; }

        public Grouping() {
            GroupDisplayNames = new ReadOnlyCollection<string>(headers);
        }
        public int GetGroupIndex(string key) {
            return headers.IndexOf(key);
        }
    }
}
