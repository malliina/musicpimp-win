using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Mle.Tiles {
    public class TileUtil {
        public static readonly string Text = "text";
        public static readonly string ImageTagName = "image";
        public static readonly string SrcAttribute = "src";

        public virtual void BeforeMediumWideUpdate(XmlDocument dest, IList<Uri> imageUris) {

        }
        private TileTemplateType mediumTemplate;
        private TileTemplateType wideImageTemplate;
        private TileTemplateType wideImageCollectionTemplate;
        public TileUtil(TileTemplateType mediumTemplate, TileTemplateType wideImageTemplate, TileTemplateType wideImageCollectionTemplate) {
            this.mediumTemplate = mediumTemplate;
            this.wideImageTemplate = wideImageTemplate;
            this.wideImageCollectionTemplate = wideImageCollectionTemplate;
        }
        public void UpdateLiveTiles(IList<Uri> imageUris) {
            if(imageUris.Count == 0) {
                return;
            }
            // square

            var squareXml = GetTemplate(mediumTemplate);
            // sets the first image in the list as the square tile image
            TileUtil.SetImage(squareXml, imageUris.First());

            // wide

            // if there are 5 (or more) images, displays an image collection in the wide tile, otherwise displays just one image
            XmlDocument wideXml = null;
            if(imageUris.Count < 5) {
                wideXml = GetTemplate(wideImageTemplate);
                SetImage(wideXml, imageUris.First());
            } else {
                wideXml = GetTemplate(wideImageCollectionTemplate);
                SetImageCollection(wideXml, imageUris);
            }
            Embed(squareXml, wideXml);
            BeforeMediumWideUpdate(wideXml, imageUris);
            Update(wideXml);
        }

        protected static XmlDocument GetTemplate(TileTemplateType templateType) {
            return TileUpdateManager.GetTemplateContent(templateType);
        }
        // embeds part of src in dest
        public static void Embed(XmlDocument src, XmlDocument dest) {
            var node = dest.ImportNode(src.GetElementsByTagName("binding").Item(0), true);
            dest.GetElementsByTagName("visual").Item(0).AppendChild(node);
        }
        public static void Update(XmlDocument tileUpdate) {
            Update(tileUpdate, TimeSpan.FromSeconds(0));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tileUpdate"></param>
        /// <param name="expiration">an expiration of 0 seconds means "does not expire"</param>
        public static void Update(XmlDocument tileUpdate, TimeSpan expiration) {
            var notification = new TileNotification(tileUpdate);
            if(expiration.TotalSeconds > 0) {
                notification.ExpirationTime = DateTimeOffset.UtcNow.Add(expiration);
            }
            var text = notification.Content.GetXml();
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
        }
        
        public static void SetImage(XmlDocument template, Uri imageUri) {
            var imageNode = template.GetElementsByTagName(ImageTagName);
            ((XmlElement)imageNode[0]).SetAttribute(SrcAttribute, imageUri.OriginalString);
        }
        public static void SetImageCollection(XmlDocument template, IList<Uri> imageUris) {
            var imageNodes = template.GetElementsByTagName(TileUtil.ImageTagName);
            for(int i = 0; i < imageUris.Count && i < 5; ++i) {
                ((XmlElement)imageNodes[i]).SetAttribute(TileUtil.SrcAttribute, imageUris[i].OriginalString);
            }
        }
    }
}
