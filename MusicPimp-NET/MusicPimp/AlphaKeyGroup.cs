using System.Collections.Generic;

namespace Mle.Phone.Xaml.Controls {
    public class AlphaKeyGroup<T> : List<T> {
        //const string TracksGroupKey = "\uD83C\uDF10";
        /// <summary>
        /// The delegate that is used to get the key information.
        /// </summary>
        /// <param name="item">An object of type T</param>
        /// <returns>The key value to use for this object</returns>
        public delegate string GetKeyDelegate(T item);

        /// <summary>
        /// The Key of this group.
        /// </summary>
        public string Key { get; private set; }
        public bool IsImage { get; private set; }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="key">The key for this group.</param>
        public AlphaKeyGroup(string key, bool isImage) {
            Key = key;
            IsImage = isImage;
        }

        /// <summary>
        /// Create a list of AlphaGroup<T> with keys set by a SortedLocaleGrouping.
        /// </summary>
        /// <param name="slg">The </param>
        /// <returns>The items source for a LongListSelector</returns>
        private static List<AlphaKeyGroup<T>> CreateGroups(Grouping grouping) {
            List<AlphaKeyGroup<T>> list = new List<AlphaKeyGroup<T>>();

            foreach (var key in grouping.GroupDisplayNames) {
                var isImage = key == Grouping.SongGroupHeader;
                list.Add(new AlphaKeyGroup<T>(key, isImage));
            }

            return list;
        }

        /// <summary>
        /// Create a list of AlphaGroup<T> with keys set by a SortedLocaleGrouping.
        /// </summary>
        /// <param name="items">The items to place in the groups.</param>
        /// <param name="ci">The CultureInfo to group and sort by.</param>
        /// <param name="getKey">A delegate to get the key from an item.</param>
        /// <param name="sort">Will sort the data if true.</param>
        /// <returns>An items source for a LongListSelector</returns>
        public static List<AlphaKeyGroup<T>> CreateGroups(IEnumerable<T> items, GetKeyDelegate getKey) {
            Grouping slg = new Grouping();
            List<AlphaKeyGroup<T>> list = CreateGroups(slg);

            foreach (T item in items) {
                int index = 0;
                index = slg.GetGroupIndex(getKey(item));
                if (index >= 0 && index < list.Count) {
                    list[index].Add(item);
                }
            }
            return list;
        }

    }
}
