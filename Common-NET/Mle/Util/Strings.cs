using System;
using System.Text;

namespace Mle.Util {
    public class Strings {
        public static string encode(string input) {
            var inputBytes = toBytes(input);
            return Convert.ToBase64String(inputBytes);
        }
        public static string decode(string input) {
            var inputBytes = Convert.FromBase64String(input);
            return toString(inputBytes);
        }
        // helpers
        public static byte[] toBytes(string input) {
            return Encoding.UTF8.GetBytes(input);
        }
        public static string toString(byte[] input) {
            return Encoding.UTF8.GetString(input, 0, input.Length);
        }
        public static byte[] AsciiStringToBytes(string s) {
            byte[] retval = new byte[s.Length];
            for(int ix = 0; ix < s.Length; ++ix) {
                char ch = s[ix];
                if(ch <= 0x7f) retval[ix] = (byte)ch;
                else retval[ix] = (byte)'?';
            }
            return retval;
        }
    }
}
