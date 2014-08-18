using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Mle.IO;
using Mle.MusicPimp.Tiles;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq;

namespace Test_Store {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void TestPaths() {
            Func<string, string> rootGetter = PathHelper.Instance.RootFolderName;
            Assert.AreEqual(String.Empty, "a".Substring(1));
            var root1 = rootGetter("a\\b\\c");
            var root2 = rootGetter("d/e/f");
            var root3 = rootGetter("g/h/");
            var root4 = rootGetter("i");
            var root5 = rootGetter("j/");
            Assert.AreEqual("a", root1);
            Assert.AreEqual("d", root2);
            Assert.AreEqual("g", root3);
            Assert.AreEqual("i", root4);
            Assert.AreEqual("j", root5);
        }
        [TestMethod]
        public void TestDropRoot() {
            Func<string, string> rootDropper = PathHelper.Instance.DropRoot;
            var drop1 = rootDropper("a/b/c");
            var drop2 = rootDropper("d");
            var drop3 = rootDropper("/e/f");
            var drop4 = rootDropper("/g");
            var drop5 = rootDropper("h\\i");
            var drop6 = rootDropper("\\j\\k");
            Assert.AreEqual("b/c", drop1);
            Assert.AreEqual(String.Empty, drop2);
            Assert.AreEqual("f", drop3);
            Assert.AreEqual(String.Empty, drop4);
            Assert.AreEqual("i", drop5);
            Assert.AreEqual("k", drop6);
        }
        [TestMethod]
        public async Task DownloadDiscoGsCover() {
            var service = StoreCoverService.Instance;
            var localCoverUri = await service.GetOrDownloadCover("Iron Maiden", "Powerslave");
            Assert.IsNotNull(localCoverUri);
            StorageFile file = await StoreFileUtils.GetFile(localCoverUri);
            var size = await file.Size();
            Assert.AreEqual(40407u, size);
        }
    }
}
