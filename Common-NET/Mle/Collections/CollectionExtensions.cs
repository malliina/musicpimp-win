using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.Collections {
    public static class CollectionExtensions {
        public static async Task<IEnumerable<T>> FilterAsync<T>(this IEnumerable<T> src, Func<T, Task<Boolean>> predicate) {
            var ret = new List<T>();
            foreach(var item in src) {
                var pass = await predicate(item);
                if(pass) {
                    ret.Add(item);
                }
            }
            return ret;
        }
        public static IEnumerable<T> Tail<T>(this IEnumerable<T> src) {
            return src.Skip(1);
        }
        public static T Head<T>(this IEnumerable<T> src) {
            return src.First();
        }
        public static string MkString<T>(this IEnumerable<T> src, string separator) {
            var size = src.Count();
            if(size == 0) return String.Empty;
            else if(size == 1) return "" + src.Head();
            else return src.Head() + separator + src.Tail().MkString(separator);
        }
    }
}
