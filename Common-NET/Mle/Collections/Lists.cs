using System.Collections.Generic;
using System.Linq;
using Mle.Collections;

namespace Mle.Collections {
    public class Lists {
        public static List<T> Interleave<T>(List<T> left, List<T> right) {
            if(left.Count == 0) return right;
            else if(right.Count == 0) return left;
            else {
                var list = new List<T>() { left.Head(), right.Head() };
                list.AddRange(Interleave(left.Tail().ToList(), right.Tail().ToList()));
                return list;
            }
        }
    }
}
