using Mle.Messaging;
using Mle.Util;
using System;
using System.Threading.Tasks;
using Windows.Phone.System.UserProfile;

namespace Mle.MusicPimp.Util {
    public class LockScreenRequest {
        public static async Task<bool> RequestThenSetLockScreen() {
            var accessGranted = await LockScreenUtil.EnsureOrRequestLockScreenAccess();
            if(accessGranted) {
                var uri = await LockScreenHelper.GetRandomLockScreenImage();
                try {
                    // might throw
                    LockScreen.SetImageUri(uri);
                } catch(Exception e) {
                    // the value does not fall between the expected range error, wtf?
                    MessagingService.Instance.Send("Unable to update lock screen image: " + e.Message);
                }
            }
            return accessGranted;
        }
    }
}
