using Mle.MusicPimp.Pimp;
using Mle.Xaml.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Mle.Network;
using Microsoft.Phone.Info;
using Mle.MusicPimp.Iap;

namespace Mle.MusicPimp.ViewModels {
    public class PhoneIAP : IapVM {
        private static PhoneIAP current = null;
        public static PhoneIAP Current {
            get {
                if(current == null)
                    current = new PhoneIAP();
                return current;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>normally if this device's ANID2 is entitled to MusicPimp Premium, exceptionally otherwise or if validation fails</returns>
        public async Task ValidatePremiumBasedOnAnid2() {
            if(FeedbackMessage != HasPremiumMessage) {
                var anid2 = UserExtendedProperties.GetValue("ANID2") as string;
                if(anid2 != null) {
                    // this POST request will return OK for a premium ANID2, non-OK (i.e. it will throw) for a non-premium ANID2
                    await client.PostJson(new ValidateAnid2Command(anid2).Json(), codeResource);
                    // Returned normally, therefore the user owns MusicPimp Premium
                    FeedbackMessage = HasPremiumMessage;
                    UsageController.Instance.EnablePremium();
                }
            }
        }
        protected override Task OnStatusUpdated() {
            return ValidatePremiumBasedOnAnid2();
        }
    }
}
