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
    public partial class MainSettingsPage : PhoneApplicationPage {
        private SettingsOverview viewModel;
        public MainSettingsPage() {
            InitializeComponent();
            viewModel = SettingsOverview.Instance;
            DataContext = viewModel;
            var t = viewModel.Limits.CalculateConsumedGb();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            viewModel.UpdateLockScreen();
            base.OnNavigatedTo(e);
        }
    }
}