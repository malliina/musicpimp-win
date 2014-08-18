using Mle.MusicPimp.Audio;
using Mle.MusicPimp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Split Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234234

namespace Mle.MusicPimp.Xaml {
    /// <summary>
    /// A page that displays a group title, a list of items within the group, and details for
    /// the currently selected item.
    /// </summary>
    public sealed partial class Player : BasePage {

        public StoreNowPlaying ViewModel {
            get { return StoreNowPlaying.Instance; }
        }

        public StorePlayerManager PlayerManager {
            get { return StorePlayerManager.Instance; }
        }
        public BasePlayer MusicPlayer {
            get { return PlayerManager.Player; }
        }

        public Player() {
            DataContext = ViewModel;
            this.InitializeComponent();
            Loaded += Player_Loaded;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Player_Loaded(object sender, RoutedEventArgs e) {
            PlayerManager.ActiveEndpointChanged += PlayerManager_ActiveEndpointChanged;
            await SyncPlayerUI();
            ScrollToCurrentPlaylistTrack();
        }

        private async void PlayerManager_ActiveEndpointChanged(MusicEndpoint obj) {
            // reloads the view with the newly selected player
            DataContext = null;
            DataContext = ViewModel;
            if (MusicPlayer.IsEventBased) {
                ViewModel.StopPolling();
            } else {
                ViewModel.StartPolling();
            }
            // asks server for status of new player
            await ViewModel.Player.UpdateNowPlaying();
        }
        private async Task SyncPlayerUI() {
            if (MusicPlayer != null && !MusicPlayer.IsEventBased) {
                await MusicPlayer.UpdateNowPlaying();
                ViewModel.StartPolling();
            }
        }
        private void ScrollToCurrentPlaylistTrack() {
            var songs = MusicPlayer.Playlist.Songs;
            var selected = songs.FirstOrDefault(item => item.IsSelected);
            if (selected != null) {
                var target = selected;
                if (selected.Index > 0 && selected.Index < songs.Count) {
                    target = songs.ElementAt(selected.Index - 1);
                }
                itemListView.ScrollIntoView(target, ScrollIntoViewAlignment.Leading);
            }
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
            base.OnNavigatingFrom(e);
            PlayerManager.ActiveEndpointChanged -= PlayerManager_ActiveEndpointChanged;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            base.OnNavigatedFrom(e);
            if (!MusicPlayer.IsEventBased) {
                ViewModel.StopPolling();
            }
        }

        #region Page state management

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState) {
            // TODO: Assign a bindable group to this.DefaultViewModel["Group"]
            // TODO: Assign a collection of bindable items to this.DefaultViewModel["Items"]


            if (pageState == null) {
                // When this is a new page, select the first item automatically unless logical page
                // navigation is being used (see the logical page navigation #region below.)
                //if (!this.UsingLogicalPageNavigation() && this.itemsViewSource.View != null) {
                //    this.itemsViewSource.View.MoveCurrentToFirst();
                //}
            } else {
                // Restore the previously saved state associated with this page
                //if (pageState.ContainsKey("SelectedItem") && this.itemsViewSource.View != null) {
                // TODO: Invoke this.itemsViewSource.View.MoveCurrentTo() with the selected
                //       item as specified by the value of pageState["SelectedItem"]
                //}
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState) {
            //if (this.itemsViewSource.View != null) {
            //    var selectedItem = this.itemsViewSource.View.CurrentItem;
            // TODO: Derive a serializable navigation parameter and assign it to
            //       pageState["SelectedItem"]
            //}
        }

        #endregion

        #region Logical page navigation

        // Visual state management typically reflects the four application view states directly
        // (full screen landscape and portrait plus snapped and filled views.)  The split page is
        // designed so that the snapped and portrait view states each have two distinct sub-states:
        // either the item list or the details are displayed, but not both at the same time.
        //
        // This is all implemented with a single physical page that can represent two logical
        // pages.  The code below achieves this goal without making the user aware of the
        // distinction.

        /// <summary>
        /// Invoked to determine whether the page should act as one logical page or two.
        /// </summary>
        /// <param name="viewState">The view state for which the question is being posed, or null
        /// for the current view state.  This parameter is optional with null as the default
        /// value.</param>
        /// <returns>True when the view state in question is portrait or snapped, false
        /// otherwise.</returns>
        private bool UsingLogicalPageNavigation(ApplicationViewState? viewState = null) {
            if (viewState == null) viewState = ApplicationView.Value;
            return viewState == ApplicationViewState.FullScreenPortrait ||
                viewState == ApplicationViewState.Snapped;
        }

        /// <summary>
        /// Invoked when an item within the list is selected.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is Snapped)
        /// displaying the selected item.</param>
        /// <param name="e">Event data that describes how the selection was changed.</param>
        void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            // Invalidate the view state when logical page navigation is in effect, as a change
            // in selection may cause a corresponding change in the current logical page.  When
            // an item is selected this has the effect of changing from displaying the item list
            // to showing the selected item's details.  When the selection is cleared this has the
            // opposite effect.
            if (this.UsingLogicalPageNavigation()) {
                this.InvalidateVisualState();
            }
            //ViewModel.AppBar.Update(itemListView);
        }

        /// <summary>
        /// Invoked when the page's back button is pressed.
        /// </summary>
        /// <param name="sender">The back button instance.</param>
        /// <param name="e">Event data that describes how the back button was clicked.</param>
        protected override void GoBack(object sender, RoutedEventArgs e) {
            if (this.UsingLogicalPageNavigation() && itemListView.SelectedItem != null) {
                // When logical page navigation is in effect and there's a selected item that
                // item's details are currently displayed.  Clearing the selection will return to
                // the item list.  From the user's point of view this is a logical backward
                // navigation.
                this.itemListView.SelectedItem = null;
            } else {
                // When logical page navigation is not in effect, or when there is no selected
                // item, use the default back button behavior.
                base.GoBack(sender, e);
            }
        }

        /// <summary>
        /// Invoked to determine the name of the visual state that corresponds to an application
        /// view state.
        /// </summary>
        /// <param name="viewState">The view state for which the question is being posed.</param>
        /// <returns>The name of the desired visual state.  This is the same as the name of the
        /// view state except when there is a selected item in portrait and snapped views where
        /// this additional logical page is represented by adding a suffix of _Detail.</returns>
        protected override string DetermineVisualState(ApplicationViewState viewState) {
            // Update the back button's enabled state when the view state changes
            var logicalPageBack = this.UsingLogicalPageNavigation(viewState) && this.itemListView.SelectedItem != null;
            var physicalPageBack = this.Frame != null && this.Frame.CanGoBack;
            //this.DefaultViewModel["CanGoBack"] = logicalPageBack || physicalPageBack;

            // Determine visual states for landscape layouts based not on the view state, but
            // on the width of the window.  This page has one layout that is appropriate for
            // 1366 virtual pixels or wider, and another for narrower displays or when a snapped
            // application reduces the horizontal space available to less than 1366.
            if (viewState == ApplicationViewState.Filled ||
                viewState == ApplicationViewState.FullScreenLandscape) {
                var windowWidth = Window.Current.Bounds.Width;
                if (windowWidth >= 1366) return "FullScreenLandscapeOrWide";
                return "FilledOrNarrow";
            }

            // When in portrait or snapped start with the default visual state name, then add a
            // suffix when viewing details instead of the list
            var defaultStateName = base.DetermineVisualState(viewState);
            return logicalPageBack ? defaultStateName + "_Detail" : defaultStateName;
        }

        #endregion

        private void OnPlaylistTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
            itemListView.SelectedItem = null;
        }

        private async void Slider_PointerCaptureLost(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) {
            await MusicPlayer.seek(seekSlider.Value);
        }

        private async void VolumeSlider_PointerCaptureLost(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) {
            MusicPlayer.IsMute = false;
            await MusicPlayer.SetVolume((int)volumeSlider.Value);
        }
    }
}
