using Windows.Security.Cryptography.Certificates;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Mle.MusicPimp.Http {
    public class HttpTests {
        public void Bla() {
            var filter = new HttpBaseProtocolFilter();
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
            var httpClient = new HttpClient(filter);
        }
    }
}
