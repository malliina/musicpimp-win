using Mle.MusicPimp.ViewModels;
using Mle.Tiles;
using System;
using System.Collections.Generic;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Mle.MusicPimp.Tiles {
    public class Tiles81 : Tiles {
        protected Tiles81()
            : base(TileTemplateType.TileWide310x150SmallImageAndText02,
            TileTemplateType.TileSquare150x150Text03) {

        }
        public override void BeforeSquareWideUpdate(XmlDocument doc, MusicItem track, Uri imageUri) {
            var largeXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare310x310ImageAndText02);
            TileUtil.SetImage(largeXml, imageUri);
            SetMeta(largeXml, track, includeAlbum: false);
            TileUtil.Embed(src: largeXml, dest: doc);
        }
        public override void UpdateImageCollection(IList<Uri> images) {
            TileUtil81.Instance.UpdateLiveTiles(images);
        }
    }
}
