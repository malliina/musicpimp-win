using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;

namespace Mle.Util {
    public class SecurityUtil {
        private static readonly string SID = "LOCAL=user";
        private static BinaryStringEncoding UTF8 = BinaryStringEncoding.Utf8;

        public static async Task<string> Encrypt(string plainText) {
            var provider = new DataProtectionProvider(SID);
            var buffer = CryptographicBuffer.ConvertStringToBinary(plainText, encoding: UTF8);
            var encrypted = await provider.ProtectAsync(buffer);
            return Convert.ToBase64String(encrypted.ToArray());
        }
        public static async Task<string> Decrypt(string encrypted) {
            var provider = new DataProtectionProvider(SID);
            var bytes = Convert.FromBase64String(encrypted);
            var buffer = bytes.AsBuffer();
            var plainBuffer = await provider.UnprotectAsync(buffer);
            var plainText = CryptographicBuffer.ConvertBinaryToString(UTF8, plainBuffer);
            return plainText;
        }
        //public static void SaveCredential(string resource, string user, string pass) {
        //    var vault = new PasswordVault();
        //    vault.Add(new PasswordCredential(resource, user, pass));
        //}
        //public static PasswordCredential LoadCredential(string resource) {
        //    var vault = new PasswordVault();
        //}
    }
}
