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
    public partial class LockScreenPage : PhoneApplicationPage {
        public LockScreenPage() {
            InitializeComponent();
            DataContext = LockScreen.Instance;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            LockScreen.Instance.CheckIsAppProvider();
            base.OnNavigatedTo(e);
        }
    }
}