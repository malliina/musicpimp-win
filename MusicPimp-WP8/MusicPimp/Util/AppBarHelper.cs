using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System;
using System.Collections.Generic;

namespace Mle.MusicPimp.Util {
    public class AppBarHelper {
        public string assetHome = "/Assets/AppBar/WP7/";

        public ApplicationBarIconButton selectAppBarButton;
        public ApplicationBarIconButton addToPlaylistAppBarButton;
        public ApplicationBarIconButton playAllAppBarButton;
        public ApplicationBarIconButton downloadAppBarButton;

        public List<ApplicationBarIconButton> multiSelectButtons;
        public List<ApplicationBarIconButton> singleSelectButtons;
        protected List<ApplicationBarIconButton> currentButtons;

        private LongListMultiSelector selector;
        private IApplicationBar bar;
        private PimpViewModel AppModel { get { return PimpViewModel.Instance; } }

        public AppBarHelper() {
            selectAppBarButton = NewAppBarButton(assetHome + "ApplicationBar.Select.png", "select", SelectApplicationBar_Click);
            downloadAppBarButton = NewAppBarButton(assetHome + "download.png", "download", DownloadMulti);
            addToPlaylistAppBarButton = NewAppBarButton(assetHome + "appbar.add.rest.png", "to playlist", AddToPlaylistMulti);
            playAllAppBarButton = NewAppBarButton(assetHome + "appbar.transport.play.rest.png", "play", PlayMulti);
            multiSelectButtons = new List<ApplicationBarIconButton>(new ApplicationBarIconButton[] { 
                selectAppBarButton, downloadAppBarButton, playAllAppBarButton, addToPlaylistAppBarButton 
            });
            singleSelectButtons = new List<ApplicationBarIconButton>(new ApplicationBarIconButton[] { 
                selectAppBarButton
            });
        }
        public void Init(LongListMultiSelector selector, IApplicationBar bar) {
            this.selector = selector;
            this.bar = bar;
            SetAppBarButtons(singleSelectButtons);
        }
        public void SetAppBarButtons(List<ApplicationBarIconButton> buttons) {
            if(buttons != currentButtons) {
                bar.Buttons.Clear();
                buttons.ForEach(btn => bar.Buttons.Add(btn));
                currentButtons = buttons;
            }
        }
        public ApplicationBarIconButton NewAppBarButton(string iconUri, string text, EventHandler clickHandler) {
            var btn = new ApplicationBarIconButton();
            btn.IconUri = new Uri(iconUri, UriKind.Relative);
            btn.Text = text;
            btn.Click += clickHandler;
            return btn;
        }
        private void WithMulti(Action<IEnumerable<MusicItem>> code) {
            var items = selector.SelectedItems;
            if(items != null) {
                var musicItems = TypeHelpers.CollectionOf<MusicItem>(items);
                code(musicItems);
            }
        }
        private void DownloadMulti(object sender, EventArgs e) {
            WithMulti(async items => {
                foreach(var item in items) {
                    await AppModel.Downloader.ValidateThenSubmitDownload(item);
                }
            });
        }
        private void AddToPlaylistMulti(object sender, EventArgs e) {
            WithMulti(async items => await AppModel.AddToPlaylistRecursively(items));
        }
        private void PlayMulti(object sender, EventArgs e) {
            WithMulti(async items => await AppModel.PlayAll(items));
        }
        private void SelectApplicationBar_Click(object sender, EventArgs e) {
            selector.IsSelectionEnabled = !selector.IsSelectionEnabled;
        }
        public void UpdateMusicLibraryAppBarButtons() {
            var appBarButtons = selector.IsSelectionEnabled ? multiSelectButtons : singleSelectButtons;
            SetAppBarButtons(appBarButtons);
        }
    }
}
