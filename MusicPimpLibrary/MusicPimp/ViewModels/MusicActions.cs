using Mle.MusicPimp.Network;
using Mle.ViewModels;
using Mle.Xaml;
using Mle.Xaml.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public class MusicActions : ViewModelBase {
        public AppBarController AppBar { get; private set; }
        private ObservableCollection<MusicItem> selected;
        public ObservableCollection<MusicItem> Selected {
            get { return selected; }
            set { SetProperty(ref selected, value); }
        }
        public ICommand PlaySelected { get; private set; }
        public ICommand AddSelected { get; private set; }
        public ICommand DeleteSelected { get; private set; }
        public ICommand AddEndpoint { get; private set; }
        public ICommand DownloadSelected { get; private set; }

        public MusicActions() {
            AppBar = new AppBarController();
            Selected = new ObservableCollection<MusicItem>();
            Selected.CollectionChanged += (s, e) => SelectionChanged();
            //DeleteSelected = new UnitCommand(() => DeleteAll(Selected));
            //PlaySelected = new AsyncUnitCommand(() => PlayAll(Selected));
            //AddSelected = new AsyncUnitCommand(() => AddToPlaylistRecursively(Selected));
            DownloadSelected = new AsyncUnitCommand(() => PimpStoreDownloader.Instance.SubmitAll(Selected));
        }
        private void SelectionChanged() {
            AppBar.Update(Selected);
            OnPropertyChanged("CanDeleteSelection");
        }
        //protected void DeleteAll(IEnumerable<MusicItem> items) {
        //    foreach(var item in items) {
        //        Delete(item);
        //    }
        //}
        //protected void Delete(MusicItem song) {
        //    try {
        //        var shouldRemove = song.IsSourceLocal;
        //        LocalLibrary.Delete(song.Path);
        //        // refreshes the items in the current view
        //        if(shouldRemove) {
        //            MusicFolder.MusicItems.Remove(song);
        //        }
        //    } catch(Exception e) {
        //        Send("Unable to delete. Perhaps the file is in use. " + e.Message);
        //    }
        //}
    }
}
