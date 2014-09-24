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
using Mle.Pages;
using Mle.MusicPimp.Messaging;
using Mle.MusicPimp.Audio;
using Mle.Util;

namespace Mle.MusicPimp.Xaml {
    public partial class Playlist : BasePhonePage {
        private PlaylistVM vm;
        public Playlist() {
            InitializeComponent();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e) {
            var meta = DeserializeQuery<SavedPlaylistMeta>(PageParams.META, new SavedPlaylistMeta("0", "nonexistent", 0));
            vm = new PlaylistVM(meta);
            DataContext = vm;
            base.OnNavigatedTo(e);
            await vm.Update();
        }
        private async void Delete_Click(object sender, EventArgs e) {
            if(vm != null) {
                await vm.DeleteCurrent();
            }
            TryGoBack();
        }

        private async void Play_Click(object sender, EventArgs e) {
            if(vm != null) {
                await vm.PlayCurrent();
            }
        }
    }
}