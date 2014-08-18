using Mle.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace Mle.Network {
    public class OAuthBase : AbstractOAuthBase {
        protected override string GenerateSignatureUsingHash(string data, byte[] key) {
            MacAlgorithmProvider hmacsha1 = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);
            IBuffer dataToSign = Strings.AsciiStringToBytes(data).AsBuffer();
            CryptographicKey cryptoKey = hmacsha1.CreateKey(key.AsBuffer());
            IBuffer signature = CryptographicEngine.Sign(cryptoKey, dataToSign);
            return CryptographicBuffer.EncodeToBase64String(signature);
        }
    }
}
