using System;
using System.Security.Cryptography;

namespace Mle.Util {
    public class StringUtil {
        public static string protectToBase64(string input) {
            var bytes = protect(input);
            return Convert.ToBase64String(bytes);
        }
        public static string unprotectFromBase64(string input) {
            var bytes = Convert.FromBase64String(input);
            return unprotect(bytes);
            
        }
        public static byte[] protect(string input) {
            return ProtectedData.Protect(Strings.toBytes(input), null);
        }
        public static string unprotect(byte[] input) {
            return Strings.toString(ProtectedData.Unprotect(input, null));
        }
    }
}
