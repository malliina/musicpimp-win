using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace tests {
    [TestClass]
    public class NetworkedTests {
        [TestMethod]
        public void SerializeMusicItem() {
            var item = new MusicItem() {
                Name = "testName",
                Artist = "testArtist",
                Album = "testAlbum",
                Duration = TimeSpan.FromSeconds(125),
                Source = new Uri("http://test.com"),
                Size = 4000000,
                Id = "testId",
                IsDir = false,
                Path = "testPath"
            };
            var cmd = new TrackChangedEvent() {
                Event = "track_changed",
                track = new PimpTrack(item)
            };
            var str = Json.SerializeToString(cmd);
            Debug.WriteLine(str);
            Assert.AreEqual("test", str);
        }
        private Uri RemoveCredentialsFromQueryParams(Uri uri) {
            var uriString = uri.OriginalString;
            var qStart = uriString.IndexOf('?');
            if(qStart < 0) {
                return uri;
            }
            var q = uri.Query;
            if(q.StartsWith("?")) {
                q = q.Substring(1);
            }
            var kvs = q.Split('&');
            var otherKvs = kvs.Where(kv => kv.Length > 1 && !kv.StartsWith("u=") && !kv.StartsWith("p=")).ToList();
            var newQuery = string.Join("&", otherKvs);
            var newUriString = uriString.Substring(0, qStart) + "?" + newQuery;
            return new Uri(newUriString, UriKind.Absolute);
        }
    }
}
