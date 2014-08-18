using System;
using System.Threading.Tasks;
using Windows.Phone.System.UserProfile;

namespace Mle.Util {
    public class LockScreenUtil {
        public static async Task<bool> EnsureOrRequestLockScreenAccess() {
            if(LockScreenManager.IsProvidedByCurrentApplication) {
                return true;
            } else {
                // If you're not the provider, this call will prompt the user for permission.
                // Calling RequestAccessAsync from a background agent is not allowed.
                var result = await LockScreenManager.RequestAccessAsync();
                return result == LockScreenRequestResult.Granted;
            }
        }

        // The following code example shows the new URI schema.
        // ms-appdata points to the root of the local app data folder.
        // ms-appx points to the Local app install folder, to reference resources bundled in the XAP package.
        //var schema = isAppResource ? "ms-appx:///" : "ms-appdata:///Local/";
        //var uri = new Uri(schema + filePathOfTheImage, UriKind.Absolute);
    }
}
