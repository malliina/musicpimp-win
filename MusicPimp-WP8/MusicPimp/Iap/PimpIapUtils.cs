using Microsoft.Phone.Controls;
using Mle.Iap;
using Mle.Messaging;
using Mle.MusicPimp.Messaging;
using Mle.MusicPimp.Xaml;
using Mle.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mle.MusicPimp.Iap {
    public class PimpIapUtils : IapUtils {
        private static PimpIapUtils instance = null;
        public static PimpIapUtils Instance {
            get {
                if(instance == null)
                    instance = new PimpIapUtils();
                return instance;
            }
        }
        protected PimpIapUtils() {

        }
        public override void EnableLicense(string productId) {
            if(productId == PremiumHelper.premiumProductId) {
                UsageController.Instance.EnablePremium();
            } else {

            }
        }
        public override Task SuggestPremium() {
            var premiumDialog = new CustomMessageBox() {
                Caption = IapStrings.SuggestPremiumTitle,
                Message = IapStrings.SuggestPremiumContent,
                LeftButtonContent = IapStrings.SuggestPremiumOkText,
                RightButtonContent = IapStrings.SuggestPremiumCancelText
            };
            premiumDialog.Dismissed += (s1, e1) => {
                switch(e1.Result) {
                    case CustomMessageBoxResult.LeftButton:
                        PageNavigationService.Instance.Navigate(PageNames.IAP);
                        break;
                    case CustomMessageBoxResult.RightButton:
                        break;
                    case CustomMessageBoxResult.None:
                        break;
                }
            };
            return PhoneUtil.OnUiThread(premiumDialog.Show);
        }
    }
}
