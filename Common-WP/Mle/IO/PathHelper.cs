using Mle.IO.Local;
using System.IO;

namespace Mle.IO {
    public class PathHelper : IPathHelper {
        private static PathHelper instance = null;
        public static PathHelper Instance {
            get {
                if(instance == null)
                    instance = new PathHelper();
                return instance;
            }
        }
        protected PathHelper() {

        }
        public override string ExtensionOf(string file) {
            return Path.GetExtension(file);
        }
        public override string FileNameOf(string file) {
            return Path.GetFileName(file);
        }
        public override string FileNameWithoutExtension(string file) {
            return Path.GetFileNameWithoutExtension(file);
        }
        public override string DirectoryNameOf(string dir) {
            return Path.GetDirectoryName(dir);
        }

    }
}
