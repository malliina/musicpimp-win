using Mle.IO;
using Mle.IO.Local;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace Mle.MusicPimp.Local {

    public abstract class StoreLocalLibraryBase : LocalLibraryBase {
        public override IPathHelper Paths { get; set; }
        public StorageFolder RootFolder { get; protected set; }
        public abstract StoreFileUtils StoreFileUtil { get; }

        public StoreLocalLibraryBase(StorageFolder rootFolder)
            : base(Settings.Instance) {
            RootFolder = rootFolder;
            Paths = PathHelper.Instance;
        }
        public override FileUtilsBase FileUtil {
            get { return StoreFileUtil; }
        }
        public override Task<bool> ContainsFile(string relativeFile) {
            return RootFolder.FileExists(relativeFile);
        }
        public override async Task<bool> ContainsFolder(string relativeFolder) {
            return relativeFolder == String.Empty
                 || (await RootFolder.FolderExists(relativeFolder));
        }
        /// <summary>
        /// Uses TagLib
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public override SongInfo ReadId3(string filePath, Stream stream) {
            var songInfo = Id3Reader.ReadId3(filePath, stream);
            if(songInfo == null) {
                songInfo = FromFile(filePath);
            }
            return songInfo;
        }
        public override async Task Delete(List<string> songs) {
            foreach(var relativePath in songs) {
                var filePath = AppLocalFolderFileUtils.WindowsSeparators(relativePath);
                var file = await RootFolder.GetFileAsync(filePath);
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            await DeleteEmptyFolders();
        }
        private async Task DeleteEmptyFolders() {
            var folders = await RootFolder.ListFolders(recursive: true);
            folders.Reverse();
            foreach(var folder in folders) {
                if(await folder.IsEmpty()) {
                    await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
            }
            // TODO refactor: create event
            await MusicItemsModel.Instance.ResetAndRefreshRoot();
        }
        public override async Task DeleteLeastPlayed(int count = 20) {
            var files = await RootFolder.GetFilesAsync(CommonFileQuery.OrderBySearchRank, 0, (uint)count);
            foreach(var file in files) {
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }
        public override async Task DeleteAll() {
            await DeleteLeastPlayed(int.MaxValue);
            await DeleteEmptyFolders();
        }
        protected override async Task<IEnumerable<MusicItem>> GetSongs(string root, string folder) {
            var ret = new List<MusicItem>();
            var files = await StoreFileUtil.ListFiles(root + folder);
            foreach(var file in files) {
                if(Paths.ExtensionOf(file.Name) == ".mp3") {
                    var virtualPath = folder + file.Name;
                    var size = (long)(await file.Size());
                    var item = await BuildMusicItem(virtualPath, StoreFileUtil.UriFor(file), size);
                    ret.Add(item);
                }
            }
            return ret;
        }
        private string DirectoryOf(string path) {
            if(path.Contains("/") || path.Contains("\\")) {
                return Paths.DirectoryNameOf(path);
            } else {
                return String.Empty;
            }
        }
    }
}
