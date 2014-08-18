using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Mle.MusicPimp.Controls;
using Mle.MusicPimp.ViewModels;
using Mle.MusicPimp.Xaml;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MusicPimp.Xaml {
    public partial class MusicFiles : PimpMainPage {
        private ApplicationBarIconButton selectAppBarButton;
        private List<ApplicationBarIconButton> multiSelectButtons;

        // library browsing
        protected ApplicationBarIconButton addToPlaylistAppBarButton;
        protected ApplicationBarIconButton playAllAppBarButton;
        protected ApplicationBarIconButton downloadAppBarButton;

        public MusicFiles() {
            InitializeComponent();
            InitializeMultiSelectButtons();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs args) {
            AppModel.FolderLoaded += AppModel_FolderLoaded;
            base.OnNavigatedTo(args);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            AppModel.FolderLoaded -= AppModel_FolderLoaded;
            base.OnNavigatedFrom(e);
        }
        private void AppModel_FolderLoaded() {
            ScrollToPreviousLongListPosition();
        }
        // needed for longlistMULTIselector only
        private void ScrollToPreviousLongListPosition() {
            var item = AppModel.CurrentScrollPosition();
            if(item != null) {
                // Trying to scroll to a nonexistent list item fails with an ArgumentException, so we suppress.
                // However, by checking MusicItems.Contains as above, this should never happen.
                // The item we try to scroll to may have been deleted, or the library may have been reset.
                // We could try to check whether the item exists in the selector, but it's not too easy. 
                // Directly searching LongListSelector.ItemsSource will not work, because the items may be grouped.
                Utils.Suppress<Exception>(() => MusicItemLongListSelector.ScrollTo(item));
            }
        }
        protected override Pivot MainPivot() {
            return musicPivot;
        }
        protected void InitializeMultiSelectButtons() {
            selectAppBarButton = NewAppBarButton(assetHome + "ApplicationBar.Select.png", "select", SelectApplicationBar_Click);
            downloadAppBarButton = NewAppBarButton(assetHome + "download.png", "download", DownloadMulti);
            addToPlaylistAppBarButton = NewAppBarButton(assetHome + "appbar.add.rest.png", "to playlist", AddToPlaylistMulti);
            playAllAppBarButton = NewAppBarButton(assetHome + "appbar.transport.play.rest.png", "play", PlayMulti);
            multiSelectButtons = new List<ApplicationBarIconButton>(new ApplicationBarIconButton[] { 
                selectAppBarButton, downloadAppBarButton, playAllAppBarButton, addToPlaylistAppBarButton 
            });
        }
        protected override List<ApplicationBarIconButton> SingleSelectButtons() {
            var ret = base.SingleSelectButtons();
            ret.Insert(0, selectAppBarButton);
            return ret;
        }

        protected override void UpdateMusicLibraryAppBarButtons() {
            var appBarButtons = MusicItemLongListSelector.IsSelectionEnabled ? multiSelectButtons : SingleSelectButtons();
            SetAppBarButtons(appBarButtons);
        }
        private void DownloadMulti(object sender, EventArgs e) {
            WithMulti(async items => {
                foreach(var item in items) {
                    await AppModel.Downloader.ValidateThenSubmitDownload(item);
                }
            });
        }
        private void SelectApplicationBar_Click(object sender, EventArgs e) {
            MusicItemLongListSelector.IsSelectionEnabled = !MusicItemLongListSelector.IsSelectionEnabled;
        }
        protected void AddToPlaylistMulti(object sender, EventArgs e) {
            WithMulti(async items => await AppModel.AddToPlaylistRecursively(items));
        }
        protected void PlayMulti(object sender, EventArgs e) {
            WithMulti(async items => await AppModel.PlayAll(items));
        }
        private void WithMulti(Action<IEnumerable<MusicItem>> code) {
            var items = MusicItemLongListSelector.SelectedItems;
            if(items != null) {
                var musicItems = TypeHelpers.CollectionOf<MusicItem>(items);
                code(musicItems);
            }
        }

        private void OnMultiSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            UpdateMusicLibraryAppBarButtons();
        }
        /// <summary>
        /// note that this callback is not called if the user clicks back, remaining on the same pivot,
        /// or returns to the pivot by holding the back button to get back to the app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e) {
            PrepareSelectedPivot();
        }

        private void OnMusicItemTap(object sender, System.Windows.Input.GestureEventArgs e) {
            HandleMusicItemSelected(sender, e);
        }
        private void Test_Click(object sender, EventArgs e) {
            GoToSharedPage(typeof(TestPage).Name);
        }
        private async void trackSlider_LostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e) {
            await MusicPlayer.seek(trackSlider.Value);
        }
        // hack
        private void ContextMenu_Unloaded(object sender, RoutedEventArgs e) {
            ContextMenu contextMenu = (sender as ContextMenu);
            contextMenu.ClearValue(FrameworkElement.DataContextProperty);
        }
        private void Downloads_Click(object sender, EventArgs e) {
            GoToSharedPage(typeof(Downloads).Name);
        }
        private void About_Click(object sender, EventArgs e) {
            GoToSharedPage(typeof(AboutFeedback).Name);
        }
        private void Settings_Click(object sender, EventArgs e) {
            GoToProjectPage("MusicPimp-WP8", typeof(MainSettingsPage).Name);
        }

        private void Beam_Click(object sender, EventArgs e) {
            GoToProjectPage("MusicPimp-WP8", typeof(BarcodePage).Name);
        }
    }
}