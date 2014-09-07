using Mle.MusicPimp.ViewModels;
using Mle.Util;
using Mle.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Mle.MusicPimp.Xaml {
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class MusicItems : BasePage {

        public MusicItemsModel Model { get; private set; }

        public MusicItems() {
            Model = MusicItemsModel.Instance;
            this.InitializeComponent();
            Loaded += (s, e) => NavigateToRememberedPosition();
        }
        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session. This will be null the first time a page is visited.</param>
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState) {
            var folderId = navigationParameter as string;
            await Model.ParseAndLoad(folderId);
        }
        private void NavigateToRememberedPosition() {
            var item = Model.CurrentScrollPosition();
            if(item != null) {
                // Might throw? Not sure, but I suppress anyway as this is merely convenience if it works.
                Utils.Suppress<Exception>(() => itemGridView.ScrollIntoView(item, ScrollIntoViewAlignment.Leading));
            }
        }

        private void OnGridTapped(object sender, TappedRoutedEventArgs e) {
            itemGridView.SelectedItem = null;
        }

        private void OnListTapped(object sender, TappedRoutedEventArgs e) {
            itemListView.SelectedItem = null;
        }

        private void HelpItemClicked(object sender, ItemClickEventArgs e) {
            var item = (TitledImageItem)e.ClickedItem;
            item.OnClicked();
        }
    }
}
