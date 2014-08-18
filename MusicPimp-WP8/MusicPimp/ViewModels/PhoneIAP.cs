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
        public async Task ValidatePremiumBasedOnAnid2() {
            if(FeedbackMessage != HasPremiumMessage) {
                var anid2 = UserExtendedProperties.GetValue("ANID2") as string;
                if(anid2 != null) {
                    await client.PostJson(new ValidateAnid2Command(anid2).Json(), codeResource);
                    FeedbackMessage = HasPremiumMessage;
                }
            }
        }
        protected override Task OnStatusUpdated() {
            return ValidatePremiumBasedOnAnid2();
        }
    }
}
