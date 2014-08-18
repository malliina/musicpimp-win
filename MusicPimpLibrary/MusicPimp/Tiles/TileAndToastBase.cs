using Mle.MusicPimp.Util;
using Mle.MusicPimp.ViewModels;
using Mle.Tiles;
using Mle.Util;
using System;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Mle.Collections;
using Windows.UI.Xaml.Media.Imaging;
using Mle.Concurrent;

namespace Mle.MusicPimp.Tiles {
    public abstract class TileAndToastBase : TileAndToastManager {
        protected static Uri DefaultImageUri { get; private set; }
        //public static Uri AbsoluteDefaultImageUri { get; private set; }

        public TileAndToastBase()
            : base(StorePlayerManager.Instance,StoreCoverService.Instance, new Uri("ms-appx:///MusicPimpLibrary/Assets/Store/guitar-558x558.png", UriKind.Absolute)) {
            DefaultImageUri = new Uri("/Assets/Store/guitar-558x558.png", UriKind.Relative);
        }
        public override Task SetBackground(Uri uri) {
            RootPageViewModel.Instance.SetBackgroundUri(uri);
            return AsyncTasks.Noop();
        }
        protected async Task<Uri> GetCoverUriOrDefault(MusicItem track) {
            var maybeUri = await Covers.TryGetCover(track);
            return maybeUri != null ? maybeUri : DefaultImageUri;
        }
        protected async Task<Uri> SetImageAndMeta(XmlDocument doc, MusicItem track) {
            var uri = await SetImage(doc, track);
            SetMeta(doc, track);
            return uri;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="track"></param>
        /// <returns>the URI of the tile image</returns>
        protected async Task<Uri> SetImage(XmlDocument doc, MusicItem track) {
            var imageUri = await GetCoverUriOrDefault(track);
            TileUtil.SetImage(doc, imageUri);
            return imageUri;
        }
        protected void SetMeta(XmlDocument doc, MusicItem track, bool includeAlbum = true) {
            var textNodes = doc.GetElementsByTagName(TileUtil.Text);
            if(includeAlbum) {
                SetTexts(textNodes, track.Name, track.Album, track.Artist);
            } else {
                SetTexts(textNodes, track.Name, track.Artist);
            }
        }
        protected void SetTexts(XmlNodeList textNodes, params string[] content) {
            int textsCount = Math.Min(content.Length, 3);
            //var textNodes = doc.GetElementsByTagName(TileUtil.Text);
            for(int i = 0; i < textsCount; ++i) {
                textNodes[i].InnerText = Utils.EmptyIfNull(content[i]);
            }
        }
    }
}
