using Microsoft.Phone.BackgroundAudio;
using Mle.MusicPimp.Controls;
using Mle.MusicPimp.Pimp;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using Mle.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Navigation;

namespace MusicPimp.Xaml {
    public partial class TestPage : AsyncPhoneApplicationPage {
        public BackgroundAudioPlayer Player { get { return BackgroundAudioPlayer.Instance; } }
        private TestViewModel model = null;
        public TestPage() {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (DataContext == null) {
                model = new TestViewModel();
                DataContext = model;
            }
            base.OnNavigatedTo(e);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            MessageBox.Show("Player: " + Player.PlayerState);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            var dir = "a/b/c";
            var dirName = Path.GetDirectoryName(dir);
            MessageBox.Show("Dir: " + dir + ", dirname: " + dirName);
        }
    }
}