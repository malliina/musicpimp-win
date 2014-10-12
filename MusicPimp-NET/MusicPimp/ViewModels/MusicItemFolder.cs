using Mle.Phone.Xaml.Controls;
using Mle.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace Mle.MusicPimp.ViewModels {
    public class MusicItemFolder : WebAwareLoading {
        protected static readonly string EmptyFolderMessage = "no MP3s in this folder";
        public event Action FolderLoaded;

        public string FolderId { get; private set; }

        public string DisplayablePath { get; private set; }

        public MusicItem LatestSelection { get; set; }

        private ObservableCollection<MusicItem> musicItems;
        public ObservableCollection<MusicItem> MusicItems {
            get { return musicItems; }
            set {
                this.SetProperty(ref this.musicItems, value);
                initGroupedItems();
                OnPropertyChanged("IsEmpty");
                OnPropertyChanged("ShouldGroup");
                OnPropertyChanged("Items");
                OnPropertyChanged("ShowHelp");
                OnPropertyChanged("ShowGrouped");
                OnPropertyChanged("ShowFlat");
            }
        }

        private List<AlphaKeyGroup<MusicItem>> groupedMusicItems;
        public List<AlphaKeyGroup<MusicItem>> GroupedMusicItems {
            get { return groupedMusicItems; }
            private set { this.SetProperty(ref this.groupedMusicItems, value); }
        }
        public object Items {
            get {
                if(ShouldGroup) {
                    return groupedMusicItems;
                } else {
                    return MusicItems;
                }
            }
        }
        public bool ShouldGroup {
            get { return MusicItems.Count > 30 && MusicItems.Any(item => item.IsDir); }
        }
        public bool IsEmpty {
            get { return MusicItems.Count == 0; }
        }
        public bool IsEmptyAndLoading {
            get { return IsEmpty && IsLoading; }
        }
        // used by WP7 due to LongListSelector.IsFlatList not being bindable
        public bool ShowGrouped {
            get { return ShouldGroup && ShowResult; }
        }
        public bool ShowFlat {
            get { return !ShouldGroup && ShowResult; }
        }
        // constructors
        public MusicItemFolder(string id, string path)
            : this(id, path, new ObservableCollection<MusicItem>()) {
        }
        protected MusicItemFolder(string id, string path, ObservableCollection<MusicItem> items) {
            FolderId = id;
            DisplayablePath = path;
            MusicItems = items;
            MusicItems.CollectionChanged += MusicItems_CollectionChanged;
        }

        private void MusicItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged("IsEmptyAndLoading");
        }
        public MusicItem GetItem(string relativePath) {
            return MusicItems.FirstOrDefault(i => i.Path == relativePath);
        }
        private void initGroupedItems() {
            if(ShouldGroup) {
                GroupedMusicItems = AlphaKeyGroup<MusicItem>.CreateGroups(
                    MusicItems,
                    GroupKeyOf);
            } else {
                GroupedMusicItems = new List<AlphaKeyGroup<MusicItem>>();
            }
        }
        private string GroupKeyOf(MusicItem item) {
            if(item.IsDir) {
                var lowerCase = item.Name.ToLowerInvariant();
                // crashes if item.Name.Length == 0
                if(char.IsDigit(lowerCase, index: 0)) {
                    return Grouping.DigitGroupHeader;
                } else {
                    return lowerCase.ToCharArray()[0].ToString();
                }
            } else {
                return Grouping.SongGroupHeader;
            }
        }
        protected void UpdateMusicItemsViews(bool sortRequired = false) {
            OnPropertyChanged("IsEmptyAndLoading");
            OnPropertyChanged("ShowHelp");
            if(sortRequired) {
                MusicItems = new ObservableCollection<MusicItem>(MusicItems.OrderBy(SortKey));
                //Debug.WriteLine("Items: " + MusicItems.Count);
            } else {
                UpdateMusicItemsList();
            }
        }
        protected void UpdateMusicItemsList() {
            OnPropertyChanged("ShouldGroup");
            OnPropertyChanged("ShowGrouped");
            OnPropertyChanged("ShowFlat");
            initGroupedItems();
            OnPropertyChanged("Items");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns>a sort key where directories are first, then based on name</returns>
        public static string SortKey(MusicItem item) {
            string prefix = item.IsDir ? "a" : "b";
            return prefix + item.Name;
        }
        public static string DirOnlySortKey(MusicItem item) {
            return item.IsDir ? "a" + item.Name : "b";
        }
        protected override void OnIsLoadingChanged(bool loading) {
            UpdateMusicItemsViews();
            OnFolderLoaded();
        }
        protected override void OnFeedbackMessageChanged(string msg) {
            OnPropertyChanged("ShowHelp");
            OnPropertyChanged("ShowGrouped");
            OnPropertyChanged("ShowFlat");
        }
        private void OnFolderLoaded() {
            if(FolderLoaded != null) {
                FolderLoaded();
            }
        }
    }
}
