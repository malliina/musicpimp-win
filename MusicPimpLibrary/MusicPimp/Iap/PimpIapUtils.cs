using Mle.Iap;
using Mle.Messaging;
using Mle.MusicPimp.Messaging;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

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
        public override async Task SuggestPremium() {
            var dialog = new MessageDialog(IapStrings.SuggestPremiumContent, IapStrings.SuggestPremiumTitle);
            UICommand okBtn = new UICommand(IapStrings.SuggestPremiumOkText);
            okBtn.Invoked = OnGetPremium;
            dialog.Commands.Add(okBtn);

            UICommand cancelBtn = new UICommand(IapStrings.SuggestPremiumCancelText);
            cancelBtn.Invoked = OnNotInterested;
            dialog.Commands.Add(cancelBtn);

            await dialog.ShowAsync();
        }
        private void OnGetPremium(IUICommand cmd) {
            // navigates to IAP page
            PageNavigationService.Instance.Navigate(PageNames.IAP);
        }
        private void OnNotInterested(IUICommand cmd) {
            // noops; dismisses dialog
        }

        public override void EnableLicense(string productId) {
            if(productId == PremiumHelper.premiumProductId) {
                UsageController.Instance.EnablePremium();
            } else {

            }
        }
    }
}
