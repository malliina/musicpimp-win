using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Mle.Tiles {
    public class TileUtil81 : TileUtil {
        private static TileUtil81 instance = null;
        public static TileUtil81 Instance {
            get {
                if(instance == null)
                    instance = new TileUtil81();
                return instance;
            }
        }
        protected TileUtil81()
            : base(TileTemplateType.TileSquare150x150Image,
                TileTemplateType.TileWide310x150Image,
                TileTemplateType.TileWide310x150ImageCollection) { }
        public override void BeforeMediumWideUpdate(XmlDocument dest, IList<Uri> imageUris) {
            XmlDocument largeXml = null;
            if(imageUris.Count < 5) {
                largeXml = GetTemplate(TileTemplateType.TileSquare310x310Image);
                SetImage(largeXml, imageUris.First());
            } else {
                largeXml = GetTemplate(TileTemplateType.TileSquare310x310ImageCollection);
                SetImageCollection(largeXml, imageUris);
            }
            Embed(largeXml, dest);
        }
    }
}
