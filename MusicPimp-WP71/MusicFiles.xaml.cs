using Microsoft.Phone.Controls;
using Mle.MusicPimp.Phone.Controls;
using Mle.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Pages {
    public partial class MusicFiles : PimpMainPage {
        public MusicFiles() {
            InitializeComponent();
            SetDefaultAppBarButtons();
        }
        private LongListSelector MusicItemLongList {
            get { return AppModel.MusicFolder.ShouldGroup ? GroupedList : FlatList; }
        }
        protected override Pivot MainPivot() {
            return musicPivot;
        }
        #region playlist

        //protected override void ScrollPlaylistTo(PlaylistMusicItem targetItem) {
        //    PlaylistLongListSelector.ScrollTo(targetItem);
        //}

        #endregion

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e) {
            PrepareSelectedPivot();
        }
        private void Downloads_Click(object sender, EventArgs e) {
            GoToSharedPage(typeof(Downloads).Name);
        }
        private void About_Click(object sender, EventArgs e) {
            GoToSharedPage(typeof(AboutFeedback).Name);
        }
        private void OnMusicItemTap(object sender, System.Windows.Input.GestureEventArgs e) {
            HandleMusicItemSelected(sender, e);
        }
        private void Test_Click(object sender, EventArgs e) {
            GoToSharedPage(typeof(TestPage).Name);
        }

        private async void trackSlider_LostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e) {
            await PimpViewModel.Instance.NowPlayingModel.Player.seek(trackSlider.Value);
        }

        private void OnMusicSourceChanged(object sender, SelectionChangedEventArgs e) {
            UpdateEndpoint(PhoneLibraryManager.Instance, userChanged: e.RemovedItems.Count > 0);
        }

        private void OnPlaybackDeviceChanged(object sender, SelectionChangedEventArgs e) {
            UpdateEndpoint(PhonePlayerManager.Instance, userChanged: e.RemovedItems.Count > 0);
        }

        private void ContextMenu_Unloaded(object sender, System.Windows.RoutedEventArgs e) {
            ContextMenu contextMenu = (sender as ContextMenu);
            contextMenu.ClearValue(FrameworkElement.DataContextProperty);
        }
    }
}