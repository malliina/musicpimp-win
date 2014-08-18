using Mle.MusicPimp.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Mle.MusicPimp.Tiles {
    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/windows/apps/hh868254.aspx
    /// </summary>
    public class Toasts : TileAndToastBase {
        private static Toasts instance = null;
        public static Toasts Instance {
            get {
                if(instance == null)
                    instance = new Toasts();
                return instance;
            }
        }
        /// <summary>
        /// Sends a toast notification displaying metadata of the given track.
        /// </summary>
        /// <param name="track">track to display in toast notification</param>
        /// <returns></returns>
        public override async Task Update(MusicItem track) {
            Uri maybeCover = await Covers.TryGetCover(track);
            if(maybeCover != null) {
                await SetBackground(maybeCover);
            }
            var xml = await SetXmlContent(track, maybeCover);
            SetSilent(xml);
            Show(xml);
        }
        private async Task<XmlDocument> SetXmlContent(MusicItem track, Uri maybeCover) {
            if(maybeCover != null) {
                var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);
                await SetImageAndMeta(toastXml, track);
                return toastXml;
            } else {
                var textToastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText04);
                SetMeta(textToastXml, track);
                return textToastXml;
            }
        }
        private void SetSilent(XmlDocument toastXml) {
            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("silent", "true");
            toastNode.AppendChild(audio);
        }
        private void Show(XmlDocument toastXml) {
            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public override Task UpdateNoTrack() {
            return TaskEx.FromResult(0);
        }
    }
}
