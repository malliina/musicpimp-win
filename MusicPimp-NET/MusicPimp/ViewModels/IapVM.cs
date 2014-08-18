using Mle.Concurrent;
using Mle.Iap;
using Mle.MusicPimp.Iap;
using Mle.Network;
using Mle.Util;
using Mle.ViewModels;
using Mle.Xaml.Commands;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public class IapVM : WebAwareLoading {
        public static readonly string
            LoadingStatus = "Loading purchase status...",
            GetStatusFailed = "Unable to verify purchase status. Please try again later.",
            HasPremiumMessage = "You own MusicPimp Premium. Contact us if you have any issues, feature requests or feedback.",
            PleasePurchaseMessage = "Currently, only a limited number of tracks can be played per day. Purchase MusicPimp Premium to remove all playback limits.",
            PremiumNotAvailable = "MusicPimp Premium is currently not available for purchase. Please come back later.",
            GenericError = "An error occurred. Please try again later.";

        protected static string codeResource = "/codes";

        private static IapVM instance = null;
        public static IapVM Instance {
            get {
                if(instance == null)
                    instance = new IapVM();
                return instance;
            }
        }
        public bool IsPurchasePossible {
            get {
                return FeedbackMessage != PremiumNotAvailable
                    && FeedbackMessage != HasPremiumMessage
                    && FeedbackMessage != LoadingStatus;
            }
        }
        public string Currency {
            get {
                var regionInfo = System.Globalization.RegionInfo.CurrentRegion;
                return regionInfo.ISOCurrencySymbol;
            }
        }
        public ProductInfo PremiumInfo { get; private set; }
        public ICommand Purchase { get; private set; }
        private PremiumHelper premiumInfo;
        protected HttpClient client;

        public IapVM() {
            premiumInfo = PremiumHelper.Instance;
            client = HttpUtil.NewJsonHttpClient("pimp", "pimp");
            client.BaseAddress = new Uri("http://reg.musicpimp.org");
            Purchase = new AsyncUnitCommand(PerformPurchase);
        }

        private async Task PerformPurchase() {
            try {
                await premiumInfo.PurchasePremium();
                FeedbackMessage = HasPremiumMessage;
            } catch(IapException ie) {
                // This message is actually not shown on WP8; probably because 
                // UpdateStatusAsync updates the purchase status and is called in 
                // IapPage.OnNavigatedTo, and if the purchase fails, a navigation 
                // back to the page takes place.
                FeedbackMessage = ie.Message;
            } catch(Exception) {
                FeedbackMessage = GenericError;
            }
            OnPropertyChanged("IsPurchasePossible");
        }
        public Task UpdateStatusAsync() {
            // wrapped in WebAware so that property IsLoading is set properly 
            return WebAware(UpdateStatus2);
        }
        private async Task UpdateStatus2() {
            await WithGenericError(async () => {
                var info = await premiumInfo.PremiumInfo();
                var isPremiumAvailable = info != null;
                if(isPremiumAvailable) {
                    if(premiumInfo.HasPremium()) {
                        FeedbackMessage = HasPremiumMessage;
                    } else {
                        FeedbackMessage = PleasePurchaseMessage + " MusicPimp Premium is available for " + info.FormattedPrice + ".";
                    }
                } else {
                    FeedbackMessage = PremiumNotAvailable;
                }
                await Utils.SuppressAsync<Exception>(OnStatusUpdated);
            });
            OnPropertyChanged("IsPurchasePossible");
        }
        protected virtual Task OnStatusUpdated() {
            return AsyncTasks.Noop();
        }
        private async Task WithGenericError(Func<Task> code) {
            try {
                await code();
            } catch(Exception) {
                FeedbackMessage = GetStatusFailed;
            }
        }
    }
}
