using Mle.Util;
using System;

namespace Mle.IO.Local {
    // class Path does not exist in PCLs so this is the best we can do here
    public abstract class IPathHelper {
        public abstract string ExtensionOf(string file);
        public abstract string FileNameOf(string filePath);
        public abstract string FileNameWithoutExtension(string file);
        public abstract string DirectoryNameOf(string dirPath);

        // http://stackoverflow.com/questions/7911448/c-get-first-directory-name-of-a-relative-path
        public string RootFolderName(string path) {
            while(true) {
                string temp = DirectoryNameOf(path);
                if(String.IsNullOrEmpty(temp))
                    break;
                path = temp;
            }
            return path;
        }
        public string DropRoot(string path) {
            return DropRoot(path, RootFolderName(path));
        }
        public string DropRoot(string path, string rootFolderName) {
            path = DropFirstSeparator(path);
            var tail = path.Substring(startIndex: rootFolderName.Length);
            return DropFirstSeparator(tail);
        }
        public string DropFirstSeparator(string path) {
            return path.DropWhile(p => p.StartsWith("\\") || p.StartsWith("/"));
        }
    }
}
