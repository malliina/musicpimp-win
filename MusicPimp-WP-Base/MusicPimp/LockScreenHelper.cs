using Mle.MusicPimp.Tiles;
using Mle.Util;
using System;
using System.Threading.Tasks;
using Windows.Phone.System.UserProfile;

namespace Mle.MusicPimp {
    public class LockScreenHelper {
        public static async Task<Uri> GetRandomLockScreenImage() {
            var uri = new Uri("ms-appx:///DefaultLockScreen.png", UriKind.Absolute); // default
            var uriList = await PhoneCoverService.Instance.GetCoverCollection(count: 1);
            if(uriList.Count > 0) {
                var isoUri = uriList[0];
                if(!isoUri.IsAbsoluteUri) {
                    isoUri = new Uri("ms-appdata:///local/" + uriList[0].OriginalString, UriKind.Absolute);
                }
                uri = isoUri;
            }
            return uri;
        }
        public static async Task TrySetRandomLockScreenImage() {
            if(LockScreenManager.IsProvidedByCurrentApplication) {
                var uri = await LockScreenHelper.GetRandomLockScreenImage();
                Utils.Suppress<Exception>(() => LockScreen.SetImageUri(uri));
            }
        }
    }
}
