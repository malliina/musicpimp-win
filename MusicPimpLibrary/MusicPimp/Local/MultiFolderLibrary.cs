using Mle.Xaml.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;

namespace Mle.MusicPimp.Local {
    public class MultiFolderLibrary : MultiFolderLibraryBase {

        private static MultiFolderLibrary instance = null;
        public static MultiFolderLibrary Instance {
            get {
                if (instance == null) {
                    instance = new MultiFolderLibrary();
                }
                return instance;
            }
        }
        private static async Task<List<LocalLibraryBase>> Load() {
            var ret = new List<LocalLibraryBase>();
            var customFolders = await StoreFolderLocalLibrary.InitLibraries();
            ret.AddRange(customFolders);
            return ret;
        }
        public ICommand AddLocalFolder { get; private set; }
        public ICommand RemoveLocalFolder { get; private set; }

        protected MultiFolderLibrary()
            : base(AppLocalFolderLibrary.Instance) {
            RootEmptyMessage = "The local MusicPimp library on this device is empty. To obtain music, add a local MP3 folder or a MusicPimp server.";
            AddLocalFolder = new AsyncUnitCommand(AddFolder);
            RemoveLocalFolder = new DelegateCommand<StoreLocalLibraryBase>(Remove);
        }
        public async Task Init() {
            var libs = await Load();
            foreach (var lib in libs) {
                Libraries.Add(lib);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>the user-selected folder, if any, or null otherwise</returns>
        private async Task<StorageFolder> LetUserSelectFolder() {
            // file pickers do not work in snapped mode
            if (EnsureUnsnapped()) {
                var picker = new FolderPicker();
                picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
                picker.FileTypeFilter.Add(".mp3");
                var folder = await picker.PickSingleFolderAsync();
                if (folder != null) {
                    StoreFolderLocalLibrary.Save(folder);
                }
                return folder;
            } else {
                return null;
            }
        }
        internal bool EnsureUnsnapped() {
            // FilePicker APIs will not work if the application is in a snapped state.
            // If an app wants to show a FilePicker while snapped, it must attempt to unsnap first
            bool unsnapped = ((ApplicationView.Value != ApplicationViewState.Snapped) || ApplicationView.TryUnsnap());
            if (!unsnapped) {
                //NotifyUser("Cannot unsnap the sample.", NotifyType.StatusMessage);
            }
            return unsnapped;
        }

        public async Task AddFolder() {
            var folder = await LetUserSelectFolder();
            if (folder != null) {
                MultiFolderLibrary.Instance.Add(folder);
            }
        }
        public void Add(StorageFolder folder) {
            AddAll(new List<StorageFolder>() { folder });
        }
        public void AddAll(IEnumerable<StorageFolder> folders) {
            var folderLibraries = folders.Select(f => new StoreFolderLocalLibrary(f));
            foreach (var lib in folderLibraries) {
                Libraries.Add(lib);
            }
            Reset();
        }
        public void Remove(StoreLocalLibraryBase library) {
            StoreFolderLocalLibrary.Remove(library.RootFolder);
            Libraries.Remove(library);
            Reset();
        }
    }
}
