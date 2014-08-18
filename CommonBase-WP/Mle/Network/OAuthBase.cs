using Mle.Util;
using System;
using System.Security.Cryptography;

namespace Mle.Network {
    public class OAuthBase : AbstractOAuthBase {
        protected override string GenerateSignatureUsingHash(string data, byte[] key) {
            HMACSHA1 hmacsha1 = new HMACSHA1();
            hmacsha1.Key = key;
            return ComputeHash(data, hmacsha1);
        }
        protected string ComputeHash(string data, HashAlgorithm hashAlgorithm) {
            if(hashAlgorithm == null) {
                throw new ArgumentNullException("hashAlgorithm");
            }

            if(string.IsNullOrEmpty(data)) {
                throw new ArgumentNullException("data");
            }
            byte[] dataBuffer = Strings.AsciiStringToBytes(data);// System.Text.Encoding.ASCII.GetBytes(data);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }
    }
}
