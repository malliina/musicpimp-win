using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mle.Concurrent;
using Mle.MusicPimp.Network;
using RestSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace tests {
    [TestClass]
    public class UploadTest {
        string file = @"E:\test.txt";
        static string requestBase = "http://192.168.0.126";
        static string requestResource = "/file";
        string requestUri = requestBase + requestResource;

        //string uri = "http://localhost:9000";
        RestClient client = new RestClient("http://localhost:9000");
        [TestMethod]
        public void Reset() {
            var req = new RestRequest("/reset", Method.POST);
            var resp = client.Execute(req);
            Debug.WriteLine("Got response: " + resp.Content);
        }
        [TestMethod]
        public async Task TestHttpClientMultipartUpload() {
            var fileName = Path.GetFileName(file);
            //var str = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            //byte[] bytes = File.ReadAllBytes(file);
            //Assert.AreEqual(19, bytes.Length);
            using(var client = new HttpClient()) {
                //using(var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture))) {
                using(var content = new MultipartFormDataContent()) {
                    foreach(var param in content.Headers.ContentType.Parameters.Where(param => param.Name.Equals("boundary")))
                        param.Value = param.Value.Replace("\"", String.Empty);
                    content.Add(new StreamContent(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read)), "\"file\"", "\"" + fileName + "\"");
                    content.Headers.Add("Track", "hey");
                    string msg = await content.ReadAsStringAsync();
                    using(var httpResponse = await client.PostAsync("http://192.168.0.126:9000/file", content)) {
                        var response = await httpResponse.Content.ReadAsStringAsync();
                        Assert.AreEqual(HttpStatusCode.OK, httpResponse.StatusCode);
                        Assert.AreEqual("Thanks for file: test.txt", response);
                    }
                }
            }
            //await AsyncTasks.Noop();
        }
    }
}
