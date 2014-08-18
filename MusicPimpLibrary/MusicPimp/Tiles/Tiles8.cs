using Mle.Tiles;
using System;
using System.Collections.Generic;
using Windows.UI.Notifications;

namespace Mle.MusicPimp.Tiles {
    public class Tiles8 : Tiles {
        protected Tiles8()
            : base(TileTemplateType.TileWideSmallImageAndText02,
                TileTemplateType.TileSquareText03) { }
        public override void UpdateImageCollection(IList<Uri> images) {
            TileUtil8.Instance.UpdateLiveTiles(images);
        }
    }
}
