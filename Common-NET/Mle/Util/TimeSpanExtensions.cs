using System;

namespace Mle.Util {
    public static class TimeSpanExtensions {
        public static string ToMyFormat(this TimeSpan ts) {
            string format = ts.Hours >= 1 ? "hh\\:mm\\:ss" : "mm\\:ss";
            return ts.ToString(format);
        }
    }
}
