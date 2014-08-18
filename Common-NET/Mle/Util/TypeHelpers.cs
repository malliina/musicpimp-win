using System.Collections;
using System.Collections.Generic;

namespace Mle.Util {
    public class TypeHelpers {
        public static IEnumerable<T> CollectionOf<T>(IEnumerable items) {
            List<T> ret = new List<T>();
            foreach (var item in items) {
                ret.Add((T)item);
            }
            return ret;
        }
    }
}
