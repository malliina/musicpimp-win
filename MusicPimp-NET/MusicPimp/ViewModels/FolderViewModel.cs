using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mle.MusicPimp.ViewModels {
    public class FolderViewModel : MusicItemFolder {
        protected LibraryManager LibraryManager {
            get { return ProviderService.Instance.LibraryManager; }
        }
        private MusicLibrary MusicProvider {
            get { return LibraryManager.MusicProvider; }
        }
        public bool ShowHelp {
            get {
                return !IsLoading &&
                    FolderId == MusicProvider.RootFolderKey &&
                    LibraryManager.ActiveEndpoint == EndpointsData.ThisDevice &&
                    IsEmpty &&
                    (FeedbackMessage == EmptyFolderMessage || FeedbackMessage == MusicProvider.RootEmptyMessage);
            }
        }
        public FolderViewModel(string id, string path)
            : base(id, path) {
        }
        public async Task<string> Load() {
            await WebAware(async () => {
                MusicProvider.NewItemsLoaded += MusicProvider_NewItemsLoaded;
                try {
                    await MusicProvider.LoadFolder(FolderId, MusicItems);
                } finally {
                    MusicProvider.NewItemsLoaded -= MusicProvider_NewItemsLoaded;
                }
            });
            FeedbackMessage = DetermineFeedbackMessage();
            return FeedbackMessage;
        }

        private void MusicProvider_NewItemsLoaded(IEnumerable<MusicItem> newItems) {
            var newItemsCount = newItems.Count();
            var hasNew = newItemsCount > 0;
            var shouldSort = hasNew && MusicItems.Count > newItemsCount;
            if(hasNew) {
                UpdateMusicItemsViews(shouldSort);
            }
        }
        private string DetermineFeedbackMessage() {
            if(FeedbackMessage == null && MusicItems.Count == 0) {
                if(FolderId == MusicProvider.RootFolderKey) {
                    return LibraryManager.MusicProvider.RootEmptyMessage;
                } else {
                    return EmptyFolderMessage;
                }
            }
            return FeedbackMessage;
        }
    }
}