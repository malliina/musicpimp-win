using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace tests {
    [TestClass]
    public class PimpSample {
        [TestMethod]
        public async Task PingPimpWithHttpClient() {
            var client = new HttpClient();
            var headers = client.DefaultRequestHeaders;
            var credBytes = Encoding.UTF8.GetBytes(String.Format("{0}:{1}", "admin", "test"));
            var encodedCredentials = Convert.ToBase64String(credBytes);
            headers.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
            headers.Accept.ParseAdd("application/json");
            client.BaseAddress = new Uri("http://localhost:8456");
            var response = await client.GetAsync("/pingauth");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PingPimpWithRestSharp() {
            var client = new RestClient("http://localhost:8456");
            client.Authenticator = new HttpBasicAuthenticator("admin", "test");
            var request = new RestRequest("/pingauth", Method.GET);
            var response = client.Execute(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
