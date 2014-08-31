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

namespace Mle.MusicPimp.Xaml {
    public partial class Search : PhoneApplicationPage {
        private SearchVM vm;
        private string latestTerm = "";
        public Search() {
            InitializeComponent();
            vm = new SearchVM();
            DataContext = vm;
            Loaded += Search_Loaded;
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

        }

        private void OnMultiSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {

        }
        private void update(FrameworkElement control) {
            var be = control.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
        }
    }
}