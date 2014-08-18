using Mle.IO;
using Mle.IO.Local;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Local {
    public class PhoneLocalLibrary : Id3LocalLibraryBase {
        private static PhoneLocalLibrary instance = null;
        public static PhoneLocalLibrary Instance {
            get {
                if(instance == null)
                    instance = new PhoneLocalLibrary();
                return instance;
            }
        }
        public override FileUtilsBase FileUtil {
            get { return PhoneFileUtils.Instance; }
        }
        public override IPathHelper Paths { get; set; }
        public override string BaseMusicPath { get; protected set; }

        protected PhoneLocalLibrary()
            : base(Settings.Instance) {
            RootEmptyMessage = "The local MusicPimp library on this device is empty. Add your PC as a music endpoint to obtain music.";
            Paths = PathHelper.Instance;
            BaseMusicPath = "music\\";
        }
        public override Task<bool> ContainsFile(string relativeFile) {
            return TaskEx.Run(() => {
                return FileUtils.FileExists(BaseMusicPath + relativeFile);
            });
        }
        public override Task<bool> ContainsFolder(string relativeFolder) {
            return TaskEx.FromResult(FileUtils.DirectoryExists(BaseMusicPath + relativeFolder));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="songs"></param>
        /// <exception cref="IsolatedStorageException">if the file cannot be deleted, for example it might be in use</exception>
        public override Task Delete(List<string> songs) {
            return TaskEx.Run(() => {
                FileUtils.WithStorage(s => {
                    songs.ForEach(path => {
                        var absolutePath = BaseMusicPath + path;
                        Delete(s, absolutePath);
                    });
                });
            });
        }

        private void DeleteDirIfEmpty(IsolatedStorageFile s, string dir) {
            var subDirs = s.GetDirectoryNames(dir + "/*");
            var files = s.GetFileNames(dir + "/*");
            if(subDirs.Length == 0 && files.Length == 0 && dir != "music") {
                s.DeleteDirectory(dir);
                var parent = Paths.DirectoryNameOf(dir);
                if(parent != null && parent != String.Empty) {
                    DeleteDirIfEmpty(s, parent);
                }
            }
        }
        public override Task DeleteLeastPlayed(int count = 20) {
            return TaskEx.Run(() => {
                var deletableFilePaths = PhoneFileUtils.Instance.ListFilePaths(BaseMusicPath, recursive: true).Take(count);
                FileUtils.WithStorage(s => {
                    foreach(var file in deletableFilePaths) {
                        Delete(s, file);
                    }
                });
            });
        }
        public override Task DeleteAll() {
            return TaskEx.Run(() => {
                var paths = PhoneFileUtils.Instance.ListFilePaths(BaseMusicPath, recursive: true);
                FileUtils.WithStorage(s => {
                    foreach(var path in paths) {
                        try {
                            Delete(s, path);
                        } catch(IsolatedStorageException) {
                            // file is in use
                        }
                    }
                });
            });
        }
        private void Delete(IsolatedStorageFile s, string filePath) {
            var canDelete = false;
            // throws an isolatedstorageexception if the file cannot be opened for writing
            using(var f = s.OpenFile(filePath, FileMode.Open, FileAccess.Write)) {
                canDelete = f.CanWrite;
            }

            if(s.FileExists(filePath) && canDelete) {
                s.DeleteFile(filePath);
            }
            // delete directory if that was the last file
            var dir = Paths.DirectoryNameOf(filePath);
            DeleteDirIfEmpty(s, dir);
        }
    }
}
