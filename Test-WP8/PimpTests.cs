using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Mle.Concurrent;
using Mle.Exceptions;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using Mle.Test;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Mle {
    [TestClass]
    public class PimpTests : TestBase {
        MusicEndpoint devEndpoint = new MusicEndpoint() {
            Name = "pimp",
            Server = "desktop",
            Port = 9000,
            Username = "admin",
            Password = "test"
        };
        MusicEndpoint localEndpoint = new MusicEndpoint() {
            Name = "test endpoint",
            Server = "desktop",
            Port = 8456,
            Username = "admin",
            Password = "test"
        };
        MusicEndpoint failEndpoint = new MusicEndpoint() {
            Name = "failing endpoint",
            Server = "desktop",
            Port = 8456,
            Username = "admin",
            Password = "HAHA"
        };
        [TestMethod]
        public Task Testing() {
            Assert.AreEqual(1, 1);
            return AsyncTasks.Noop();
        }
        [TestMethod]
        public async Task FailPing() {
            var s = new PhonePimpSession(failEndpoint);
            await ThrowsExceptionAsync<ServerResponseException>(async () => {
                await s.PingAuth();
            });
        }
        [TestMethod]
        public async Task LoadFolders() {
            var s = new PhonePimpSession(localEndpoint);
            var folders = await s.RootContentAsync();
        }

        public class UploadFile {
            public UploadFile() {
                ContentType = "application/octet-stream";
            }
            public string Name { get; set; }
            public string FileName { get; set; }
            public string ContentType { get; set; }
            public Stream Stream { get; set; }
        }
    }
}
