using Mle.IO;
using Mle.MusicPimp.Audio;
using Mle.MusicPimp.Local;
using Mle.MusicPimp.Network;
using Mle.MusicPimp.Xaml;
using Mle.Xaml;
using Mle.Xaml.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public class MusicItemsModel : MusicItemsBase {
        private static MusicItemsModel instance = null;
        public static MusicItemsModel Instance {
            get {
                if (instance == null)
                    instance = new MusicItemsModel();
                return instance;
            }
        }

        public override BasePlayer MusicPlayer {
            get { return StorePlayerManager.Instance.Player; }
        }
        public override MusicLibrary MusicProvider {
            get { return StoreLibraryManager.Instance.MusicProvider; }
        }
        public StoreNowPlaying NowPlaying {
            get { return StoreNowPlaying.Instance; }
        }

        public AppBarController AppBar { get; private set; }

        private ObservableCollection<MusicItem> selected;
        public ObservableCollection<MusicItem> Selected {
            get { return selected; }
            set { SetProperty(ref selected, value); }
        }
        public bool CanDeleteSelection {
            // true if each selection is a track in app local storage
            get { return Selected.Count > 0 && !Selected.Any(i => i.IsDir || !i.IsSourceLocal || FileUtilsBase.IsLocalNonAppFile(i.Source)); }
        }
        public HelpModel Help { get; private set; }

        public ICommand PlaySelected { get; private set; }
        public ICommand AddSelected { get; private set; }
        public ICommand DeleteSelected { get; private set; }
        public ICommand AddEndpoint { get; private set; }
        public ICommand DownloadSelected { get; private set; }

        protected MusicItemsModel()
            : base(MultiFolderLibrary.Instance) {
            AppBar = new AppBarController();
            Selected = new ObservableCollection<MusicItem>();
            Selected.CollectionChanged += (s, e) => SelectionChanged();

            StoreLibraryManager.Instance.ActiveEndpointChanged += async e => await ResetAndRefreshRoot();
            MultiFolderLibrary.Instance.Libraries.CollectionChanged += async (s, e) => await ResetAndRefreshRoot();

            DeleteSelected = new UnitCommand(() => DeleteAll(Selected));
            PlaySelected = new AsyncUnitCommand(() => PlayAll(Selected));
            AddSelected = new AsyncUnitCommand(() => AddToPlaylistRecursively(Selected));
            AddEndpoint = new UnitCommand(() => PopupManager.Show(new AddEndpointPopup()));
            DownloadSelected = new AsyncUnitCommand(() => PimpStoreDownloader.Instance.SubmitAll(Selected));
            Help = new HelpModel();
        }

        private void SelectionChanged() {
            AppBar.Update(Selected);
            OnPropertyChanged("CanDeleteSelection");
        }
    }
}
