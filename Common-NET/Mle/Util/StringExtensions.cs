using System;

namespace Mle.Util {
    public static class StringExtensions {
        public static string DropWhile(this string str, Func<string, bool> predicate) {
            while (predicate(str)) {
                str = str.Substring(startIndex: 1);
            }
            return str;
        }
    }
}
