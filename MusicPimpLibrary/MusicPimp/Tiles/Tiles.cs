using Mle.MusicPimp.ViewModels;
using Mle.Tiles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Mle.MusicPimp.Tiles {
    public abstract class Tiles : TileAndToastBase {
        protected TileTemplateType wideImageText02;
        protected TileTemplateType squareText03;
        protected Tiles(TileTemplateType wideImageText02, TileTemplateType squareText03) {
            this.wideImageText02 = wideImageText02;
            this.squareText03 = squareText03;
        }
        public virtual void BeforeSquareWideUpdate(XmlDocument doc, MusicItem track, Uri imageUri) { }

        /// <summary>
        /// Cover to the left, title, album, artist to the right.
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public override async Task Update(MusicItem track) {
            // wide
            var wideXml = TileUpdateManager.GetTemplateContent(wideImageText02);
            var imageUri = await SetImageAndMeta(wideXml, track);
            // square
            var squareXml = TileUpdateManager.GetTemplateContent(squareText03);
            SetMeta(squareXml, track);

            TileUtil.Embed(squareXml, wideXml);

            BeforeSquareWideUpdate(wideXml, track, imageUri);

            var trackDuration = track.Duration;
            var trackDurationSeconds = trackDuration.TotalSeconds;
            var isTrackDurationValid = trackDurationSeconds > 0 && trackDurationSeconds < 10000;
            var expiration = isTrackDurationValid ? trackDuration : TimeSpan.FromMinutes(5);
            TileUtil.Update(wideXml, expiration);
        }
        /// <summary>
        /// Picks 5 covers randomly and shows them in an image collection tile. Square approx similarly.
        /// </summary>
        /// <returns></returns>
        public override async Task UpdateNoTrack() {
            var images = await GetImageCollection();
            UpdateImageCollection(images);
        }
        public abstract void UpdateImageCollection(IList<Uri> images);

        private async Task<IList<Uri>> GetImageCollection() {
            var images = await Covers.GetCoverCollection(count: 5);
            if(images.Count == 0) {
                images.Add(DefaultImageUri);
            }
            return images;
        }
    }
}
