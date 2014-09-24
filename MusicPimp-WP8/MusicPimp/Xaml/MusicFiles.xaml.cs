using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Mle.MusicPimp.Controls;
using Mle.MusicPimp.Xaml;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MusicPimp.Xaml {
    public partial class MusicFiles : PimpMainPage {
        protected ApplicationBarIconButton searchAppBarButton;

        public MusicFiles() {
            InitializeComponent();
            InitializeMultiSelectButtons();
            appBars.Init(MusicItemLongListSelector, ApplicationBar);
        }
        protected override void OnNavigatedTo(NavigationEventArgs args) {
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
            searchAppBarButton = appBars.NewAppBarButton(appBars.assetHome + "feature.search.png", "search", Search_Click);
        }
        protected override List<ApplicationBarIconButton> SingleSelectButtons() {
            var ret = base.SingleSelectButtons();
            //ret.Insert(0, selectAppBarButton);
            ret.Add(searchAppBarButton);
            return ret;
        }

        protected override void UpdateMusicLibraryAppBarButtons() {
            var appBarButtons = MusicItemLongListSelector.IsSelectionEnabled ? appBars.multiSelectButtons : SingleSelectButtons();
            appBars.SetAppBarButtons(appBarButtons);
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
            GoToWP8(typeof(MainSettingsPage));
        }
        private void Search_Click(object sender, EventArgs e) {
            GoToWP8(typeof(Search));
        }
        private void Beam_Click(object sender, EventArgs e) {
            GoToWP8(typeof(BarcodePage));
        }
        private void GoToWP8(Type page) {
            GoToProjectPage("MusicPimp-WP8", page.Name);
        }
    }
}