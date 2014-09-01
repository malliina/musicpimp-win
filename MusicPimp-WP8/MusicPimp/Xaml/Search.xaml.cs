using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Mle.MusicPimp.ViewModels;
using Mle.MusicPimp.Util;

namespace Mle.MusicPimp.Xaml {
    public partial class Search : PhoneApplicationPage {
        private SearchVM vm;
        private string latestTerm = "";
        private AppBarHelper appBars;
        public Search() {
            InitializeComponent();
            vm = new SearchVM();
            DataContext = vm;
            Loaded += Search_Loaded;
            appBars = new AppBarHelper();
            appBars.Init(MusicItemLongListSelector, ApplicationBar);
        }
        public void InitAppBar() {
            appBars.multiSelectButtons.ForEach(btn => ApplicationBar.Buttons.Add(btn));
        }

        void Search_Loaded(object sender, RoutedEventArgs e) {
            searchTextBox.Focus();
        }

        private async void OnTermChanged(object sender, TextChangedEventArgs e) {
            update(searchTextBox);
            var trimmed = vm.Term.Trim();
            // The TextChanged event fires twice on WP. This hack ensures search is called only once for each search term.
            if(trimmed != latestTerm) {
                latestTerm = trimmed;
                if(latestTerm.Length > 1) {
                    await vm.Search();
                }
            }
        }

        private void OnMusicItemTap(object sender, System.Windows.Input.GestureEventArgs e) {
            HandleMusicItemSelected(sender, e);
        }

        private void OnMultiSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            // appbar buttons
            appBars.UpdateMusicLibraryAppBarButtons();
        }
        protected async void HandleMusicItemSelected(object sender, System.Windows.Input.GestureEventArgs e) {
            var musicItem = ((FrameworkElement)sender).DataContext as MusicItem;
            if(musicItem != null) {
                await vm.Provider.MusicItemsBase.OnSingleMusicItemSelected(musicItem);
            }

        }
        private void update(FrameworkElement control) {
            var be = control.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
        }
    }
}