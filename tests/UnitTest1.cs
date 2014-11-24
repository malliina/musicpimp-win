using Id3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mle.Collections;
using Mle.Concurrent;
using Mle.Exceptions;
using Mle.MusicPimp.Subsonic;
using Mle.Network;
using Newtonsoft.Json;
using RestSharp.Contrib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tests {

    public class Nestor {
        public int Age { get; set; }
    }

    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void ObjectVersusString() {
            object o = "hello";
            string other = "hello";
            Assert.AreEqual(other, o);
            object o2 = "";
            Assert.AreEqual(o2.ToString(), String.Empty);
        }
        [TestMethod]
        public void UrlEncoding() {
            //var test = "A B";
            var expected = "A+B";
            var result = WebUtility.HtmlEncode(expected);
            Assert.AreEqual(expected, result);
            //var result2 = WebUtility.HtmlEncode(test);
            //Assert.AreEqual(expected, result2);
        }
        public class A {
            public int Value { get; protected set; }
            public A() {
                Value = GetValue();
            }
            protected virtual int GetValue() {
                return 1;
            }
        }
        public class B : A {
            override protected int GetValue() {
                return 2;
            }
        }
        [TestMethod]
        public void ConstSubCallTest() {
            var b = new B();
            Assert.AreEqual(2, b.Value);
        }
        [TestMethod]
        public void TypeTests() {
            var stringName = typeof(String).Name;
            Assert.AreEqual("String", stringName);
        }
        [TestMethod]
        public void EqualsTest() {
            Assert.IsTrue("a".Equals("a"));
        }
        [TestMethod]
        public void SortTest() {
            var obs = new ObservableCollection<int>() { 3, 1, 2 };
            var obs2 = obs.OrderBy(SortKey);
            Assert.AreEqual(obs.Head(), obs2.Head());
        }
        private int SortKey(int item) {
            return item;
        }
        [TestMethod]
        public void Substrings() {
            var ip = "192.168.1.101";
            var ipInfo = NetworkUtils.IpSplit(ip);
            Assert.AreEqual(101, ipInfo.LastOctet);
        }
        [TestMethod]
        public void ListInterleave() {
            var l1 = new List<int>() { 1, 2, 3 };
            var l2 = new List<int>() { 4, 5, 6 };
            var expected = new List<int>() { 1, 4, 2, 5, 3, 6 };
            Assert.IsTrue(expected.SequenceEqual(Lists.Interleave(l1, l2)));
        }

        public List<Func<CancellationToken, Task<string>>> BuildTasks() {
            var t1 = BuildTask(() => {
                Thread.Sleep(1000);
                return "aye";
            });
            var t2 = BuildTask(() => {
                Thread.Sleep(400);
                return "done sleeping";
            });
            var t3 = BuildTask(() => {
                Thread.Sleep(200);
                throw new Exception("Kaboom");
            });
            var t4 = BuildTask(() => {
                throw new Exception("poff");
            });
            return new List<Func<CancellationToken, Task<string>>>() { t1, t2, t3, t4 };
        }
        private Func<CancellationToken, Task<string>> BuildTask(Func<string> job) {
            return token => TaskEx.Run(job);
        }
        [TestMethod]
        public async Task TaskTests() {
            Debug.WriteLine("Testing tasks...");
            var winner = await AsyncTasks.FirstSuccessfulWithin(BuildTasks(), TimeSpan.FromMilliseconds(2000));
            Assert.AreEqual("done sleeping", winner);
            try {
                string noWinners = await AsyncTasks.FirstSuccessfulWithin(BuildTasks(), TimeSpan.FromMilliseconds(200));
                Debug.WriteLine(noWinners);
                Assert.Fail("Expected timeout, got success.");
            } catch(TimeoutException) {

            }
            try {
                var noWinners = await AsyncTasks.FirstSuccessfulWithin(BuildTasks().Skip(2).ToList(), TimeSpan.FromMilliseconds(5000));
                Assert.Fail("Expected failure, got success.");
            } catch(NoResultsException) {

            }
        }
        [TestMethod]
        public void Todo() {
            new Nestor {
                Age = 5
            };
        }
        [TestMethod]
        public void PathTest() {
            var p = "a/b/c.mp3";
            var dirPath = Path.GetDirectoryName(p);
            Assert.AreEqual("a\\b", dirPath);
        }
        [TestMethod]
        public void CollectionTests() {
            var list = new List<int>() { 1, 2, 3, 4, 5 };
            var nil = list.Skip(42).ToList();
            Assert.AreEqual(0, nil.Count);
            var all = list.Take(42).ToList();
            Assert.AreEqual(5, all.Count);
        }
        [TestMethod]
        public void TestStrings() {
            var prefix = "abc";
            var test = "abcdef";
            var tail = test.Substring(prefix.Length);
            Assert.AreEqual("def", tail);
        }
        [TestMethod]
        public void TestLinq() {
            var lengthCalcTimes = 0;
            List<string> names = new List<string>() { "a", "b2", "c33" };
            var nameLengths = names.Select(n => {
                lengthCalcTimes++;
                return n.Length;
            }).ToList();
            nameLengths.Count();
            nameLengths.Min();
            Assert.AreEqual(3, lengthCalcTimes);
        }
        [TestMethod]
        public void TestMethod2() {
            Debug.WriteLine("Running tests...");
            AsyncTest(async s => {
                await Task.Delay(1000);
                var res = s.ToUpper();
                Debug.WriteLine("Done: " + res);
            }).Wait();
            //await testHttpClient();
            //var resp = testRestSharp();
            //var cont = resp.Content;
            //Debug.WriteLine(cont);
            Debug.WriteLine("Done running tests.");
        }
        public async Task AsyncTest(Func<string, Task> code) {
            var str = "hello, world";
            await code(str);
        }
        private void testDirs() {
            var dir = "a/b";
            var dirName = Path.GetDirectoryName(dir);
            Debug.WriteLine(dirName);
        }
        private async Task<string> basicHttpClientTest() {
            var client = NewHttpClient();
            var resp = await client.GetAsync("/folders");
            return await resp.Content.ReadAsStringAsync();
        }
        private HttpClient NewHttpClient() {
            var username = "admin";
            var password = "test";
            var credBytes = Encoding.UTF8.GetBytes(String.Format("{0}:{1}", username, password));
            var credsEncoded = Convert.ToBase64String(credBytes);
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://desktop:9000");
            var headers = client.DefaultRequestHeaders;
            headers.Authorization = new AuthenticationHeaderValue("Basic", credsEncoded);
            headers.Accept.ParseAdd("application/json");
            headers.AcceptEncoding.ParseAdd("gzip, deflate");
            return client;
        }
        public void TestJson() {
            var json = @"{""subsonic-response"": {
 ""error"": {
  ""message"": ""The trial period for the Subsonic server is over. Please donate to get a license key. Visit subsonic.org for details."",
  ""code"": 60
 },
 ""status"": ""failed"",
 ""xmlns"": ""http://subsonic.org/restapi"",
 ""version"": ""1.8.0""
}}";
            var response = JsonConvert.DeserializeObject<SubsonicResponseContainer>(json);
            Debug.WriteLine("Response: " + response.subsonicResponse + ", error: " + response.subsonicResponse.error.message + ", obj: " + response);
        }
        public void testUrlDecode() {
            var encoded = "Nelly+Furtado%5CFolklore%5CNelly+Furtado+-+Build+You+Up.mp3";
            var decoded = HttpUtility.UrlDecode(encoded);
            Debug.WriteLine(encoded + " is decoded: " + decoded);
        }
        private void testDirectories() {
            var path = "a/b/c";
            Debug.WriteLine("directory of " + path + ": " + Path.GetDirectoryName(path));
        }
        private void testId3(string filePath) {
            if(!File.Exists(filePath)) {
                Debug.WriteLine(filePath + " does not exist");
                return;
            } else {
                Debug.WriteLine(filePath + " exists");
            }
            using(var file = new FileStream(filePath, FileMode.Open)) {
                using(var mp3 = new Mp3Stream(file)) {
                    //var t = mp3.GetTag(Id3TagFamily.Version2x);
                    //Debug.WriteLine(t.Title.Value);
                    var tags = new List<Id3Tag>(mp3.GetAllTags());
                    var tagCount = tags.Count;
                    tags.ForEach(tag => {
                        Debug.WriteLine(tag.Title.Value);
                    });
                    Debug.WriteLine("tags: " + tagCount);
                    Debug.WriteLine("duration: " + mp3.Audio.Duration.TotalSeconds);
                    Debug.WriteLine("bitrate: " + mp3.Audio.Bitrate);
                    Assert.AreEqual(506, mp3.Audio.Duration);
                }
            };
        }
        private void printNullStatus(string label, object param) {
            if(param == null) {
                Debug.WriteLine(label + "is null");
            } else {
                Debug.WriteLine(label + " is not null");
            }
        }
        private void testTime() {
            var time = TimeSpan.FromSeconds(200.42);
            var time2 = TimeSpan.FromSeconds(3600.31);

            Debug.WriteLine(time.ToString("c"));
            Debug.WriteLine(time.ToString("hh\\:mm\\:ss"));
        }
        private void testPaths() {
            Debug.WriteLine("Hello");
            var list = new List<string> { 
                "shared/media/hoohhoo/hoo.mp3", 
                "/shared/media/hoohhoo/hoo.mp3",
                "shared/media/hoohhoo/",
                "shared/media/hoohhoo"
            };
            list.ForEach(path => Debug.WriteLine(Path.GetDirectoryName(path)));
            var pathList = new List<string> { "a/b/c/d.mp3", "j/k/l.mp3", "f/g.mp3", "e.mp3" };//, 
            pathList.ForEach(analyze);
        }
        private void analyze(string path) {
            var dirPath = Path.GetDirectoryName(path);
            var album = dirPath;
            album = Path.GetFileName(album);
            var artist = String.Empty;
            if(dirPath != String.Empty)
                artist = Path.GetDirectoryName(dirPath);
            artist = Path.GetFileName(artist);
            if(artist == String.Empty) {
                artist = album;
                album = String.Empty;
            }
            Debug.WriteLine(
                "artist: " + artist + ", " +
                "album: " + album + ", " +
                "name: " + Path.GetFileName(path) + ", bye!"
                );
        }
        private string baseUrl = "http://192.168.137.230:4040/rest/{0}.view?u=admin&p=test&v=1.8.0&c=wp8app&f=json";
        public string Html(string uri) {
            var req = WebRequest.Create(uri);
            var resp = req.GetResponse();
            var stream = resp.GetResponseStream();
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        private string UriFor(string methodName) {
            return String.Format(baseUrl, methodName);
        }
        public void Html2(string uriString) {
            var webClient = new WebClient();
            webClient.DownloadStringCompleted += OnComplete;
            webClient.DownloadStringAsync(new Uri(uriString));
        }
        private void OnComplete(object o, DownloadStringCompletedEventArgs a) {
            var response = a.Result;
            Console.WriteLine(response);
        }
        public enum ApiMethod { ping };


    }
}
